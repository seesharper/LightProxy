﻿namespace LightProxy.Tests
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;

    using SimpleProxy.Tests;

    public class MethodBuilderMethodSkeleton : IMethodSkeleton
    {
        private readonly string outputPath;
        private readonly string fileName;
        private AssemblyBuilder assemblyBuilder;
        private TypeBuilder typeBuilder;

        private MethodInfo dynamicMethod;
        private MethodBuilder methodBuilder;
        private ILGenerator ilGenerator;


        public MethodBuilderMethodSkeleton(string outputPath)
        {
            this.outputPath = outputPath;
            fileName = Path.GetFileName(outputPath);
            CreateAssemblyBuilder();
            CreateTypeBuilder();
            CreateMethodBuilder();
        }

        private void CreateMethodBuilder()
        {
            methodBuilder = typeBuilder.DefineMethod(
                "DynamicMethod", MethodAttributes.Public | MethodAttributes.Static, typeof(object), new Type[] { typeof(object), typeof(object[]) });
            methodBuilder.InitLocals = true;
            ilGenerator = methodBuilder.GetILGenerator();
        }

        private void CreateAssemblyBuilder()
        {
            AppDomain myDomain = AppDomain.CurrentDomain;
            assemblyBuilder = myDomain.DefineDynamicAssembly(CreateAssemblyName(), AssemblyBuilderAccess.Save, Path.GetDirectoryName(outputPath));
        }

        private void CreateTypeBuilder()
        {
            var moduleName = Path.GetFileNameWithoutExtension(outputPath);
            ModuleBuilder module = assemblyBuilder.DefineDynamicModule(moduleName, fileName);
            typeBuilder = module.DefineType("DynamicType", TypeAttributes.Public);
        }

        private AssemblyName CreateAssemblyName()
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(outputPath);
            return new AssemblyName(fileNameWithoutExtension);
        }

        public void Verify()
        {
            Type type = typeBuilder.CreateType();
            dynamicMethod = type.GetMethod("DynamicMethod");
            assemblyBuilder.Save(fileName);
            Console.WriteLine("Saving file " + fileName);
            AssemblyAssert.IsValidAssembly(outputPath);
        }

        public ILGenerator GetILGenerator()
        {
            return ilGenerator;
        }

        public Func<object, object[], object> CreateDelegate()
        {
            Type type = typeBuilder.CreateType();
            dynamicMethod = type.GetMethod("DynamicMethod");
            assemblyBuilder.Save(fileName);
            Console.WriteLine("Saving file " + fileName);
            AssemblyAssert.IsValidAssembly(outputPath);
            var del = Delegate.CreateDelegate(typeof(Func<object, object[], object>), dynamicMethod);
            return (Func<object, object[], object>)del;            
        }

    }
}