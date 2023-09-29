namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class InterpolatedStringHandlerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
    {
        public string[] Arguments { get; }

        public InterpolatedStringHandlerArgumentAttribute(string argument) : this(new[] { argument }) { }

        public InterpolatedStringHandlerArgumentAttribute(params string[] arguments) { Arguments = arguments; }
    }
}
