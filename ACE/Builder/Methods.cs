using System.Collections.Generic;

namespace ACE
{

    public class Methods : List<Method>
    {
        public Stack<Method> Stack = new Stack<Method>();
        public Method Current
        {
            get
            {
                return Stack.Peek();
            }
        }
        public new Method Push(Method method)
        {
            base.Add(method);
            Stack.Push(method);
            return method;
        }
        public void Pop()
        {
            Stack.Pop();
        }
    }
}