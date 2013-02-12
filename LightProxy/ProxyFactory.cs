﻿[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:PrefixLocalCallsWithThis", Justification = "No inheritance")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Single source file deployment.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:FileMustHaveHeader", Justification = "Custom header.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "All public members are documented.")]

namespace LightProxy
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Represents a class that intercepts method calls.
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>
        /// Invoked when a method call is intercepted.
        /// </summary>
        /// <param name="invocationInfo">The <see cref="InvocationInfo"/> instance that 
        /// contains information about the current method call.</param>
        /// <returns>The return value from the method.</returns>
        object Invoke(InvocationInfo invocationInfo);        
    }

    /// <summary>
    /// Represents a class that is capable of creating a proxy <see cref="Type"/>.
    /// </summary>
    public interface IProxyBuilder
    {
        /// <summary>
        /// Gets a proxy <see cref="Type"/> that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.
        /// </summary>
        /// <param name="baseType">The base <see cref="Type"/> that the proxy will inherit from.</param>
        /// <param name="interfaces">A list of additional interfaces that the proxy type will implement.</param>
        /// <returns>A proxy type that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.</returns>
        Type GetProxyType(Type baseType, Type[] interfaces);

        /// <summary>
        /// Gets a proxy <see cref="Type"/> that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.
        /// </summary>
        /// <param name="baseType">The base <see cref="Type"/> that the proxy will inherit from.</param>
        /// <param name="interfaces">A list of additional interfaces that the proxy type will implement.</param>
        /// <param name="targetFactory">A function delegate use to create the target instance.</param>
        /// <returns>A proxy type that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.</returns>
        /// <remarks>
        /// The <paramref name="targetFactory"/> allows for lazy proxies that will have their target 
        /// created when the proxy type itself is created. The <paramref name="targetFactory"/> is implemented 
        /// as a static field member of the proxy type and invoked by the constructor.
        /// </remarks>
        Type GetProxyType(Type baseType, Type[] interfaces, Func<object> targetFactory);
    }

    /// <summary>
    /// Represents a class that is capable of creating proxy objects.
    /// </summary>
    public interface IProxyFactory
    {
        /// <summary>
        /// Creates a proxy object that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.
        /// </summary>
        /// <param name="baseType">The base <see cref="Type"/> that the proxy will inherit from.</param>
        /// <param name="interfaces">A list of additional interfaces that the proxy type will implement.</param>
        /// <param name="target">The target object to proxy.</param>
        /// <param name="interceptor">The <see cref="IInterceptor"/> used to intercept method calls.</param>
        /// <returns>A new object that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.</returns>
        object CreateProxy(Type baseType, Type[] interfaces, object target, IInterceptor interceptor);
    }

    /// <summary>
    /// Implemented by all proxy types.
    /// </summary>
    public interface IProxy
    {
        /// <summary>
        /// Gets or sets the proxy target.
        /// </summary>
        object Target { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Interceptor"/> used to intercept method calls.
        /// </summary>
        IInterceptor Interceptor { get; set; }
    }

    /// <summary>
    /// Represents the skeleton of a dynamic method.
    /// </summary>    
    public interface IMethodSkeleton
    {
        /// <summary>
        /// Gets the <see cref="ILGenerator"/> used to emit the method body.
        /// </summary>
        /// <returns>An <see cref="ILGenerator"/> instance.</returns>
        ILGenerator GetILGenerator();

        /// <summary>
        /// Create a delegate used to invoke the dynamic method.
        /// </summary>
        /// <returns>A function delegate.</returns>
        Func<object,object[], object> CreateDelegate();
    }

    /// <summary>
    /// Represents a class that is capable of invoking a method.
    /// </summary>
    public interface IMethodInvoker
    {
        /// <summary>
        /// Invokes the <paramref name="method"/> on the target <paramref name="instance"/> 
        /// using the given <paramref name="arguments"/>.
        /// </summary>
        /// <param name="method">The method to invoke.</param>
        /// <param name="instance">The target instance.</param>
        /// <param name="arguments">The arguments to be used when invoking the target <paramref name="method"/>.</param>
        /// <returns>The return value from the invoked method. If this is a void method, the return value is null.</returns>
        object Invoke(MethodInfo method, object instance, object[] arguments);


        Func<object, object[], object> CreateDelegate(MethodInfo methodInfo);
    }
   


    public class DynamicMethodSkeleton : IMethodSkeleton
    {
        private readonly DynamicMethod dynamicMethod = new DynamicMethod("DynamicMethod", typeof(object), new[] { typeof(object), typeof(object[]) }, typeof(MethodInvoker).Module);

        /// <summary>
        /// Gets the <see cref="ILGenerator"/> used to emit the method body.
        /// </summary>
        /// <returns>An <see cref="ILGenerator"/> instance.</returns>
        public ILGenerator GetILGenerator()
        {
            return dynamicMethod.GetILGenerator();
        }

        /// <summary>
        /// Create a delegate used to invoke the dynamic method.
        /// </summary>
        /// <returns>A function delegate.</returns>
        public Func<object, object[], object> CreateDelegate()
        {
            return (Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>));
        }
    }

    public static class DelegateBuilder
    {
        private static MethodInvoker invoker;

        public static Func<object, object[], object> CreateDelegate(MethodInfo methodInfo)
        {
            return invoker.CreateDelegate(methodInfo);
        }

    }


    public class MethodInvoker : IMethodInvoker
    {
        private static readonly Dictionary<IntPtr, Func<object, object[], object>> DelegateCache 
            = new Dictionary<IntPtr, Func<object, object[], object>>();
                
        private static readonly ConcurrentDictionary<MethodInfo, Func<object, object[], object>> Cache
            = new ConcurrentDictionary<MethodInfo, Func<object, object[], object>>();

        private static object lockRoot = new object();

        private Func<object, object[], object> cachedDelegate;
        private readonly Func<IMethodSkeleton> methodSkeletonFactory = () => new DynamicMethodSkeleton();
               
        public MethodInvoker(Func<IMethodSkeleton> methodSkeletonFactory)
        {
            this.methodSkeletonFactory = methodSkeletonFactory;
        }

        public object Invoke(MethodInfo method, object instance, object[] arguments)
        {
            Func<object, object[], object> del;
            if (!DelegateCache.TryGetValue(method.MethodHandle.Value, out del))
            {
                lock (lockRoot)
                {
                    if (!DelegateCache.TryGetValue(method.MethodHandle.Value, out del))
                    {
                        del = CreateDelegate(method);
                        DelegateCache.Add(method.MethodHandle.Value, del);
                    }
                }
            }
            return del(instance, arguments);

            return Cache.GetOrAdd(method, CreateDelegate)(instance, arguments);                                                    
        }

        public Func<object, object[], object> CreateDelegate(MethodInfo method)
        {
            var parameters = method.GetParameters();
            IMethodSkeleton methodSkeleton = methodSkeletonFactory();            
            var il = methodSkeleton.GetILGenerator();
            
            PushInstance(method, il);
            PushArguments(parameters, il);
            CallTargetMethod(method, il);
            UpdateOutAndRefArguments(parameters, il);

            PushReturnValue(method, il);
            return methodSkeleton.CreateDelegate();
        }

        private static void PushReturnValue(MethodInfo method, ILGenerator il)
        {
            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                BoxIfNecessary(method.ReturnType, il);                
            }

            il.Emit(OpCodes.Ret);
        }
        
        private static void PushArguments(ParameterInfo[] parameters, ILGenerator il)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                PushObjectValueFromArgumentArray(il, i);
                PushArgument(parameters[i], il);
            }
        }

        private static void PushArgument(ParameterInfo parameter, ILGenerator il)
        {
            Type parameterType = parameter.ParameterType;
            if (parameter.IsOut || parameter.ParameterType.IsByRef)
            {
                PushOutOrRefArgument(parameter, il);
            }
            else
            {
                UnboxOrCast(parameterType, il);
            }
        }

        private static void PushOutOrRefArgument(ParameterInfo parameter, ILGenerator il)
        {
            Type parameterType;
            parameterType = parameter.ParameterType.GetElementType();
            LocalBuilder outValue = il.DeclareLocal(parameterType);
            UnboxOrCast(parameterType, il);
            il.Emit(OpCodes.Stloc, outValue);
            il.Emit(OpCodes.Ldloca, outValue);
        }

        private static void PushObjectValueFromArgumentArray(ILGenerator il, int parameterIndex)
        {
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4, parameterIndex);
            il.Emit(OpCodes.Ldelem_Ref);
        }

        private static void CallTargetMethod(MethodInfo method, ILGenerator il)
        {
            il.Emit(OpCodes.Callvirt, method);
        }

        private static void PushInstance(MethodInfo method, ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, method.DeclaringType);
        }
       
        private static void UnboxOrCast(Type parameterType, ILGenerator il)
        {
            il.Emit(parameterType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, parameterType);
        }

        private static void UpdateOutAndRefArguments(ParameterInfo[] parameters, ILGenerator il)
        {
            int localIndex = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsOut || parameters[i].ParameterType.IsByRef)
                {
                    var parameterType = parameters[i].ParameterType.GetElementType();
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldloc, localIndex);
                    BoxIfNecessary(parameterType, il);
                    il.Emit(OpCodes.Stelem_Ref);
                    localIndex++;
                }
            }
        }

        private static void BoxIfNecessary(Type parameterType, ILGenerator il)
        {
            if (parameterType.IsValueType)
            {
                il.Emit(OpCodes.Box, parameterType);
            }
        }
    }

    /// <summary>
    /// Contains information about the current method invocation.
    /// </summary>
    public class InvocationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationInfo"/> class.
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo"/> currently being invoked.</param>
        /// <param name="target">The target object currently being invoked.</param>
        /// <param name="arguments">The arguments currently being passed to the target method.</param>
        /// <param name="proceed">The function delegate use to invoke the target method.</param>
        public InvocationInfo(MethodInfo method, object target, object[] arguments, Func<object, object[], object> proceed)
        {
            Method = method;
            Target = target;
            Arguments = arguments;
            Proceed = proceed;
        }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> currently being invoked.
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// Gets the target object currently being invoked.
        /// </summary>
        public object Target { get; private set; }

        /// <summary>
        /// Gets the arguments currently being passed to the target method.
        /// </summary>
        public object[] Arguments { get; private set; }

        /// <summary>
        /// Gets a function delegate used to invoke the target method.
        /// </summary>
        public Func<object, object[], object> Proceed { get; private set; }
    }  
    
    /// <summary>
    /// Contains information about the method being intercepted.
    /// </summary>
    internal class DelegateInfo
    {
        internal Lazy<Func<object, object[], object>> ProceedDelegate;
        
        internal MethodInfo Method;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationInfo"/> class.
        /// </summary>
        /// <param name="method">The target <see cref="MethodInfo"/> being intercepted.</param>
        internal DelegateInfo(MethodInfo method)
        {
            ProceedDelegate = new Lazy<Func<object, object[], object>>(() => DelegateBuilder.CreateDelegate(method));
            Method = method;
        }                
    }

    internal static class DelegateInfoCache
    {
        private static readonly ConcurrentDictionary<Type[], DelegateInfo> Cache 
            = new ConcurrentDictionary<Type[], DelegateInfo>(new TypeArrayComparer());
        
        public static DelegateInfo GetDelegateInfo(Type[] types, MethodInfo targetMethod)
        {
            return Cache.GetOrAdd(types, t => CreateInterceptedMethodInfo(types, targetMethod));
        }

        private static DelegateInfo CreateInterceptedMethodInfo(Type[] types, MethodInfo targetMethod)
        {
            var closedGenericMethod = targetMethod.MakeGenericMethod(types);            
            return new DelegateInfo(closedGenericMethod);
        }
    }

    internal class TypeArrayComparer : IEqualityComparer<Type[]>
    {
        public bool Equals(Type[] x, Type[] y)
        {
            return ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y));
        }

        public int GetHashCode(Type[] types)
        {
            return types.Aggregate(0, (current, type) => current ^ type.GetHashCode());
        }
    }



    /// <summary>
    /// A class that is capable of creating proxy objects.
    /// </summary>
    public class ProxyFactory : IProxyFactory
    {
        private readonly IProxyBuilder proxyBuilder = new ProxyBuilder();

        /// <summary>
        /// Creates a proxy object that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.
        /// </summary>
        /// <param name="baseType">The base <see cref="Type"/> that the proxy will inherit from.</param>
        /// <param name="interfaces">A list of additional interfaces that the proxy type will implement.</param>
        /// <param name="target">The target object to proxy.</param>
        /// <param name="interceptor">The <see cref="IInterceptor"/> used to intercept method calls.</param>
        /// <returns>A new object that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.</returns>
        public object CreateProxy(Type baseType, Type[] interfaces, object target, IInterceptor interceptor)
        {
            Type proxyType = proxyBuilder.GetProxyType(baseType, interfaces);
            var proxy = (IProxy)Activator.CreateInstance(proxyType);
            proxy.Target = target;
            proxy.Interceptor = interceptor;
            return proxy;
        }
    }

    /// <summary>
    /// A class that is capable of creating a proxy <see cref="Type"/>.
    /// </summary>
    public class ProxyBuilder : IProxyBuilder
    {
        private static readonly MethodInfo InvokeMethod;
        private static readonly MethodInfo GetTargetMethod;
        private static readonly MethodInfo SetTargetMethod;
        private static readonly MethodInfo GetInterceptorMethod;
        private static readonly MethodInfo SetInterceptorMethod;
        private static readonly MethodInfo GetMethodFromHandleMethod;
        private static readonly ConstructorInfo InvocationInfoConstructor;
        private static readonly ConstructorInfo DelegateInfoConstructor;        
        private static readonly FieldInfo ProceedDelegateField;
        private static readonly FieldInfo MethodField;

        private static readonly MethodInfo LazyGetValueMethod;

        [ThreadStatic]
        private static TypeBuildContext typeBuildContext;

        [ThreadStatic]
        private static MethodBuildContext methodBuildContext;

        static ProxyBuilder()
        {
            
            InvokeMethod = typeof(IInterceptor).GetMethod("Invoke");
            GetTargetMethod = typeof(IProxy).GetMethod("get_Target");
            SetTargetMethod = typeof(IProxy).GetMethod("set_Target");
            GetInterceptorMethod = typeof(IProxy).GetMethod("get_Interceptor");
            SetInterceptorMethod = typeof(IProxy).GetMethod("set_Interceptor");
            GetMethodFromHandleMethod = typeof(MethodBase).GetMethod("GetMethodFromHandle", new[] { typeof(RuntimeMethodHandle) });
            InvocationInfoConstructor = typeof(InvocationInfo).GetConstructors()[0];            
            DelegateInfoConstructor = typeof(DelegateInfo).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            LazyGetValueMethod = typeof(Lazy<Func<object, object[], object>>).GetProperty("Value").GetGetMethod();
            MethodField = typeof(DelegateInfo).GetField("Method", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Gets a proxy <see cref="Type"/> that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.
        /// </summary>
        /// <param name="baseType">The base <see cref="Type"/> that the proxy will inherit from.</param>
        /// <param name="interfaces">A list of additional interfaces that the proxy type will implement.</param>
        /// <returns>A proxy type that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.</returns>
        public Type GetProxyType(Type baseType, Type[] interfaces)
        {
            return CreateProxyType(baseType, interfaces);
        }
        
        /// <summary>
        /// Gets a proxy <see cref="Type"/> that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.
        /// </summary>
        /// <param name="baseType">The base <see cref="Type"/> that the proxy will inherit from.</param>
        /// <param name="interfaces">A list of additional interfaces that the proxy type will implement.</param>
        /// <param name="targetFactory">A function delegate use to create the target instance.</param>
        /// <returns>A proxy type that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.</returns>
        /// <remarks>
        /// The <paramref name="targetFactory"/> allows for lazy proxies that will have their target 
        /// created when the proxy type itself is created. The <paramref name="targetFactory"/> is implemented 
        /// as a static field member of the proxy type and invoked by the constructor.
        /// </remarks>
        public Type GetProxyType(Type baseType, Type[] interfaces, Func<object> targetFactory)
        {
            throw new NotImplementedException();
        }

        private static Type CreateProxyType(Type baseType, Type[] interfaceTypes)
        {           
            InitializeBuildContext(baseType, interfaceTypes);            
            ImplementProxyInterface();
            ImplementMethods();
            var type = typeBuildContext.TypeBuilder.CreateType();
            
#if DEBUG
            ((AssemblyBuilder)typeBuildContext.TypeBuilder.Assembly).Save("ProxyAssembly.dll");
#endif
            return type;
        }

       

        private static void InitializeBuildContext(Type baseType, Type[] interfaceTypes)
        {
            typeBuildContext = new TypeBuildContext();
            typeBuildContext.BaseType = baseType;
            typeBuildContext.InterfaceTypes = interfaceTypes;
            typeBuildContext.TypeBuilder = GetTypeBuilder(baseType, interfaceTypes);
            typeBuildContext.TargetField = DefineTargetField(typeBuildContext.TypeBuilder);
            typeBuildContext.InterceptorField = DefineInterceptorField(typeBuildContext.TypeBuilder);
            typeBuildContext.TargetMethods = GetTargetMethods(baseType, interfaceTypes);
            typeBuildContext.TargetProperties = GetTargetProperties(baseType, interfaceTypes);
            typeBuildContext.TypeInitializerGenerator = typeBuildContext.TypeBuilder.DefineTypeInitializer().GetILGenerator();
        }

        private static FieldBuilder DefineTargetField(TypeBuilder typeBuilder)
        {
            return DefinePrivateField(typeBuilder, "target", typeof(object));
        }

        private static FieldBuilder DefineInterceptorField(TypeBuilder typeBuilder)
        {
            return DefinePrivateField(typeBuilder, "interceptor", typeof(IInterceptor));
        }

        private static FieldBuilder DefinePrivateField(TypeBuilder typeBuilder, string fieldName, Type type)
        {
            return typeBuilder.DefineField(fieldName, type, FieldAttributes.Private);
        }

        private static void ImplementProxyInterface()
        {            
            typeBuildContext.TypeBuilder.AddInterfaceImplementation(typeof(IProxy));
            ImplementGetTargetMethod();
            ImplementSetTargetMethod();
            ImplementGetInterceptorMethod();
            ImplementSetInterceptorMethod();
        }

        private static void ImplementMethods()
        {
            foreach (MethodInfo method in typeBuildContext.TargetMethods)
            {
                ImplementMethod(method);
            }

            typeBuildContext.TypeInitializerGenerator.Emit(OpCodes.Ret);                      
        }

        //private static MethodBuilder ImplementMethod_old(MethodInfo targetMethod)
        //{
        //    var methodBuilder = GetMethodBuilder(targetMethod);
        //    var genericTypeParameters = CreateGenericTypeParameters(targetMethod, methodBuilder);
        //    ParameterInfo[] parameters = targetMethod.GetParameters();
        //    ILGenerator il = methodBuilder.GetILGenerator();
        //    PushInterceptorInstance(il);
        //    PushCurrentMethod(targetMethod, il, genericTypeParameters);
        //    PushTargetInstance(il);
        //    LocalBuilder argumentArray = PushArguments(parameters, il);
        //    il.Emit(OpCodes.Ldnull); // Proceed method
        //    il.Emit(OpCodes.Newobj, InvocationInfoConstructor);
        //    CallInvokeMethod(il);
        //    UpdateRefArguments(parameters, il, argumentArray);
        //    PushReturnValue(targetMethod, il);
        //    return methodBuilder;
        //}

        private static void ImplementMethod(MethodInfo targetMethod)
        {            
            InitializeMethodBuildContext(targetMethod);
            
            PushInterceptorInstance();            
            PushCurrentMethod();            
            PushTargetInstance();
            PushArguments();
            methodBuildContext.Generator.Emit(OpCodes.Ldnull); // Proceed method
            methodBuildContext.Generator.Emit(OpCodes.Newobj, InvocationInfoConstructor);
            CallInvokeMethod();            
            UpdateRefArguments();
            PushReturnValue();                       
        }

        private static void InitializeMethodBuildContext(MethodInfo targetMethod)
        {
            methodBuildContext = new MethodBuildContext();            
            methodBuildContext.MethodBuilder = GetMethodBuilder(targetMethod);            
            methodBuildContext.Generator = methodBuildContext.MethodBuilder.GetILGenerator();
            methodBuildContext.Parameters = targetMethod.GetParameters();
            methodBuildContext.ArgumentArrayField = DeclareArgumentArray();
            methodBuildContext.GenericParameters = CreateGenericTypeParameters(targetMethod, methodBuildContext.MethodBuilder);
            if (targetMethod.IsGenericMethodDefinition)
            {
                methodBuildContext.TargetMethod = targetMethod.MakeGenericMethod(methodBuildContext.GenericParameters);
            }
            else
            {
                methodBuildContext.TargetMethod = targetMethod;
            }
        }


        //private static void PushInvocationInfo(MethodInfo targetMethod, MethodBuilder methodBuilder, ILGenerator il)
        //{
        //    ParameterInfo[] parameters = targetMethod.GetParameters();
        //    if (targetMethod.IsGenericMethodDefinition)
        //    {
        //        PushCurrentGenericMethod(targetMethod, methodBuilder, il);
        //    }
        //    else
        //    {
        //        FieldBuilder delegateInfofield = UpdateTypeInitializer(targetMethod);
        //        il.Emit(OpCodes.Ldsfld, delegateInfofield);
        //        il.Emit(OpCodes.Ldfld, MethodField);
        //        PushTargetInstance(il);
        //        LocalBuilder argumentArray = PushArguments(parameters, il);
        //        il.Emit(OpCodes.Ldnull); // Proceed method
        //        il.Emit(OpCodes.Newobj, InvocationInfoConstructor);
        //    }
        //}
         
        private static void PushCurrentGenericMethod(MethodInfo targetMethod, MethodBuilder methodBuilder, ILGenerator il)
        {
            var genericTypeParameters = CreateGenericTypeParameters(targetMethod, methodBuilder);
            MethodInfo closedGenericMethod = targetMethod.MakeGenericMethod(genericTypeParameters);
            PushMethodInfo(closedGenericMethod, il);
        }

        private static GenericTypeParameterBuilder[] CreateGenericTypeParameters(MethodInfo targetMethod, MethodBuilder methodBuilder)
        {
            if (!targetMethod.IsGenericMethodDefinition)
            {
                return null;
            }

            Type[] genericArguments = targetMethod.GetGenericArguments().ToArray();
            GenericTypeParameterBuilder[] genericTypeParameters = methodBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            for (int i = 0; i < genericArguments.Length; i++)
            {
                genericTypeParameters[i].SetGenericParameterAttributes(genericArguments[i].GenericParameterAttributes);
                ApplyGenericConstraints(genericArguments[i], genericTypeParameters[i]);
            }

            return genericTypeParameters;
        }

        private static void ApplyGenericConstraints(Type genericArgument, GenericTypeParameterBuilder genericTypeParameter)
        {
            var genericConstraints = genericArgument.GetGenericParameterConstraints();
            genericTypeParameter.SetInterfaceConstraints(genericConstraints.Where(gc => gc.IsInterface).ToArray());
            genericTypeParameter.SetBaseTypeConstraint(genericConstraints.FirstOrDefault(t => t.IsClass));
        }

        private static void PushArguments()
        {
            var parameters = methodBuildContext.Parameters;
            int parameterCount = parameters.Length;
            var il = methodBuildContext.Generator;
            for (int i = 0; i < parameterCount; ++i)
            {
                Type parameterType = parameters[i].ParameterType;
                il.Emit(OpCodes.Ldloc, methodBuildContext.ArgumentArrayField);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg, i + 1);
                if (parameters[i].IsOut)
                {
                    parameterType = parameters[i].ParameterType.GetElementType();
                    if (parameterType.IsValueType)
                    {
                        il.Emit(OpCodes.Ldobj, parameterType);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldind_Ref);
                    }
                }

                if (parameterType.IsValueType || parameterType.IsGenericParameter)
                {
                    il.Emit(OpCodes.Box, parameterType);
                }

                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Ldloc, methodBuildContext.ArgumentArrayField);           
        }

        private static void PushReturnValue()
        {
            var returnType = methodBuildContext.TargetMethod.ReturnType;
            var il = methodBuildContext.Generator;
            if (returnType != typeof(void))
            {
                il.Emit(returnType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, returnType);
            }
            else
            {
                il.Emit(OpCodes.Pop);
            }

            il.Emit(OpCodes.Ret);
        }

        private static LocalBuilder DeclareArgumentArray()
        {
            var il = methodBuildContext.Generator;
            LocalBuilder argumentArray = methodBuildContext.Generator.DeclareLocal(typeof(object[]));
            il.Emit(OpCodes.Ldc_I4, methodBuildContext.Parameters.Count());
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, argumentArray);
            return argumentArray;
        }

        private static void CallInvokeMethod()
        {            
            methodBuildContext.Generator.Emit(OpCodes.Callvirt, InvokeMethod);
        }

        private static void UpdateRefArguments()
        {
            var parameters = methodBuildContext.Parameters;
            var il = methodBuildContext.Generator;
            var argumentsArray = methodBuildContext.ArgumentArrayField;
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (parameters[i].IsOut)
                {
                    Type parameterType = parameters[i].ParameterType.GetElementType();
                    il.Emit(OpCodes.Ldarg, i + 1);
                    il.Emit(OpCodes.Ldloc, argumentsArray);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldelem_Ref);
                    il.Emit(parameterType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, parameterType);
                    il.Emit(OpCodes.Stobj, parameterType);
                }
            }
        }
       
        private static void PushCurrentMethod()
        {
            var il = methodBuildContext.Generator;
            il.Emit(OpCodes.Ldtoken, methodBuildContext.TargetMethod);
            il.Emit(OpCodes.Call, GetMethodFromHandleMethod);
            il.Emit(OpCodes.Castclass, typeof(MethodInfo));
        }

        private static FieldBuilder UpdateTypeInitializer(MethodInfo targetMethod)
        {
            var fieldBuilder = typeBuildContext.TypeBuilder.DefineField(
                targetMethod.Name + "delegate",
                typeof(DelegateInfo),
                FieldAttributes.InitOnly | FieldAttributes.Private | FieldAttributes.Static);
            var il = typeBuildContext.TypeInitializerGenerator;
            PushMethodInfo(targetMethod, il);            
            il.Emit(OpCodes.Newobj, DelegateInfoConstructor);
            il.Emit(OpCodes.Stsfld, fieldBuilder);
            return fieldBuilder;
        }


        private static void PushMethodInfo(MethodInfo targetMethod, ILGenerator il)
        {
            il.Emit(OpCodes.Ldtoken, targetMethod);
            il.Emit(OpCodes.Call, GetMethodFromHandleMethod);
            il.Emit(OpCodes.Castclass, typeof(MethodInfo));
        }

        private static MethodInfo[] GetTargetMethods(Type baseType, IEnumerable<Type> interfaces)
        {
            return baseType.GetMethods().Concat(interfaces.SelectMany(i => i.GetMethods()))
                .Where(m => m.IsVirtual && !m.IsSpecialName).Distinct().ToArray();
        }

        private static PropertyInfo[] GetTargetProperties(Type baseType, IEnumerable<Type> interfaces)
        {
            return baseType.GetProperties().Concat(interfaces.SelectMany(i => i.GetProperties())).Distinct().ToArray();
        }           

        private static void ImplementGetInterceptorMethod()
        {
            InitializeMethodBuildContext(GetInterceptorMethod);            
            PushInterceptorInstance();
            methodBuildContext.Generator.Emit(OpCodes.Ret);
        }

        private static void ImplementSetInterceptorMethod()
        {
            InitializeMethodBuildContext(SetInterceptorMethod);            
            SavePropertyValue(typeBuildContext.InterceptorField);
            methodBuildContext.Generator.Emit(OpCodes.Ret);
        }

        private static void ImplementGetTargetMethod()
        {
            InitializeMethodBuildContext(GetTargetMethod);
            PushTargetInstance();
            methodBuildContext.Generator.Emit(OpCodes.Ret);
        }

        private static void ImplementSetTargetMethod()
        {
            InitializeMethodBuildContext(SetTargetMethod);            
            SavePropertyValue(typeBuildContext.TargetField);
            methodBuildContext.Generator.Emit(OpCodes.Ret);
        }

        private static void PushTargetInstance()
        {
            var il = methodBuildContext.Generator;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeBuildContext.TargetField);
        }

        private static void PushInterceptorInstance()
        {
            methodBuildContext.Generator.Emit(OpCodes.Ldarg_0);
            methodBuildContext.Generator.Emit(OpCodes.Ldfld, typeBuildContext.InterceptorField);
        }

        private static MethodBuilder GetMethodBuilder(MethodInfo targetMethod)
        {
            MethodAttributes methodAttributes = targetMethod.Attributes;

            string methodName = targetMethod.Name;
            if (targetMethod.DeclaringType.IsInterface)
            {
                methodAttributes = targetMethod.Attributes ^ MethodAttributes.Abstract;

                if (targetMethod.DeclaringType != typeBuildContext.BaseType)
                {
                    methodName = targetMethod.DeclaringType.FullName + "." + targetMethod.Name;
                }
            }

            MethodBuilder methodBuilder = typeBuildContext.TypeBuilder.DefineMethod(methodName, methodAttributes,
                                            targetMethod.ReturnType,
                                            targetMethod.GetParameters().Select(p => p.ParameterType).ToArray());

            if (targetMethod.DeclaringType != typeBuildContext.BaseType)
            {
                typeBuildContext.TypeBuilder.DefineMethodOverride(methodBuilder, targetMethod);
            }

            return methodBuilder;
        }

        private static void SavePropertyValue(FieldBuilder field)
        {
            var il = methodBuildContext.Generator;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, field);
        }

        private static AssemblyBuilder GetAssemblyBuilder()
        {
#if DEBUG            
            var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProxyAssembly.dll");
            
            var assemblybuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("ProxyAssembly"), AssemblyBuilderAccess.RunAndSave, Path.GetDirectoryName(assemblyPath));
            return assemblybuilder;
#else
            var assemblybuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("ProxyAssembly"), AssemblyBuilderAccess.Run);
            return assemblybuilder;
#endif
        }

        private static ModuleBuilder GetModuleBuilder()
        {
            AssemblyBuilder assemblyBuilder = GetAssemblyBuilder();
#if DEBUG
            return assemblyBuilder.DefineDynamicModule("ProxyAssembly", "ProxyAssembly.dll");
#else
            return assemblyBuilder.DefineDynamicModule("ProxyAssembly");
#endif
        }

        private static TypeBuilder GetTypeBuilder(Type baseType, Type[] interfaceTypes)
        {            
            ModuleBuilder moduleBuilder = GetModuleBuilder();
            const TypeAttributes TypeAttributes = TypeAttributes.Public | TypeAttributes.Class;
            var typeName = baseType.Name + "Proxy";            
            Type[] proxyInterfaceTypes = new[] { baseType }.Concat(interfaceTypes).ToArray();
            return moduleBuilder.DefineType(typeName, TypeAttributes, null, proxyInterfaceTypes);                                            
        }

        private class TypeBuildContext
        {
            public FieldBuilder TargetField { get; set; }

            public FieldBuilder InterceptorField { get; set; }

            public TypeBuilder TypeBuilder { get; set; }

            public Type BaseType { get; set; }

            public Type[] InterfaceTypes { get; set; }

            public MethodInfo[] TargetMethods { get; set; }

            public PropertyInfo[] TargetProperties { get; set; }

            public ILGenerator TypeInitializerGenerator { get; set; }

            // What about events?
        }

        private class MethodBuildContext
        {
            public MethodBuilder MethodBuilder { get; set; }

            public MethodInfo TargetMethod { get; set; }

            public ParameterInfo[] Parameters { get; set; }

            public GenericTypeParameterBuilder[] GenericParameters { get; set; }

            public ILGenerator Generator { get; set; }

            public LocalBuilder ArgumentArrayField { get; set; }
        }
    }    
}