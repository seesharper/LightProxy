[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:PrefixLocalCallsWithThis", Justification = "No inheritance")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Single source file deployment.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:FileMustHaveHeader", Justification = "Custom header.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "All public members are documented.")]

namespace LightProxy.Tests
{   
    public interface IClassWithGenericClassContraint<T> where T:class,new()
    {
        
    }
    
    public interface IClassWithReferenceTypeProperty
    {
        string Value { get; set; }
    }

    public interface IClassWithValueTypeProperty
    {
        string Value { get; set; }
    }
    
    public interface IMethodWithGenericConstraint 
    {
        void Execute<T>(T value) where T : class, new();
    }

    public interface IMethodWithNoParameters
    {
        void Execute();
    }

    public interface IMethodWithReferenceTypeParameter
    {
        void Execute(string value);
    }

    public class MethodWithReferenceTypeParameter : IMethodWithReferenceTypeParameter
    {
        public void Execute(string value)
        {
            
        }
    }

    public interface IMethodWithValueTypeParameter
    {
        void Execute(int value);
    }
 
    public interface IMethodWithNullableParameter
    {
        void Execute(int? value);
    }

    public interface IMethodWithReferenceTypeRefParameter
    {
        void Execute(ref ReferenceTypeFoo value);
    }

    public class MethodWithReferenceTypeRefParameter : IMethodWithReferenceTypeRefParameter
    {
        public void Execute(ref ReferenceTypeFoo value)
        {
            value =  new ReferenceTypeFoo() { Value = "AnotherValue" };
        }
    }

    public interface IMethodWithValueTypeRefParameter
    {
        void Execute(ref ValueTypeFoo value);
    }

    public class MethodWithValueTypeRefParameter : IMethodWithValueTypeRefParameter
    {
        public void Execute(ref ValueTypeFoo value)
        {
            value = new ValueTypeFoo { Value = "AnotherValue" };
        }
    }

    public interface IMethodWithValueTypeOutParameter
    {
        void Execute(out int value);
    }

    public interface IMethodWithReferenceTypeOutParameter
    {
        void Execute(out string value);
    }

    public interface IMethodWithReferenceTypeReturnValue
    {
        string Execute();
    }

    public interface IMethodWithValueTypeReturnValue
    {
        int Execute();
    }

    public interface IMethodWithGenericParameter
    {
        void Execute<T>(T value);
    }
        
    public class ReferenceTypeFoo
    {
        public string Value { get; set; }
    }

    public struct ValueTypeFoo
    {
        public string Value { get; set; }
    }
}