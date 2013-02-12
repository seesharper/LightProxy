[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:PrefixLocalCallsWithThis", Justification = "No inheritance")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Single source file deployment.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:FileMustHaveHeader", Justification = "Custom header.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "All public members are documented.")]

namespace LightProxy.Tests
{   
    public interface IMethodWithNoParameters
    {
        void Execute();
    }

    public interface IMethodWithReferenceTypeParameter
    {
        void Execute(string value);
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
        void Execute(ref string value);
    }

    public interface IMethodWithValueTypeRefParameter
    {
        void Execute(ref int value);
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


    public class MethodWithReferenceTypeParameter : IMethodWithReferenceTypeParameter
    {
        public void Execute(string value)
        {

        }
    }

    public class MethodWithReferenceTypeRefParameter : IMethodWithReferenceTypeRefParameter
    {
        public void Execute(ref string value)
        {
            value = "AnotherValue";
        }
    }
 
    public class MethodWithValueTypeRefParameter : IMethodWithValueTypeRefParameter
    {
        public void Execute(ref int value)
        {
            value = 52;
        }
    }
    
    public class MethodWithValueTypeOutParameter : IMethodWithValueTypeOutParameter
    {
        public void Execute(out int value)
        {
            value = 52;
        }
    }
    
    public class MethodWithReferenceTypeOutParameter : IMethodWithReferenceTypeOutParameter
    {
        public void Execute(out string value)
        {
            value = "AnotherValue";
        }
    }
}