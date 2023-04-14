using System;

public sealed class Singleton
{
    private static Singleton instance = null;
    private static readonly object lockObj = new object();

    private Singleton()
    {
        Console.WriteLine("Creating Singleton instance...");
    }

    public static Singleton Instance
    {
        get
        {
            lock (lockObj)
            {
                if (instance == null)
                {
                    instance = new Singleton();
                }
                return instance;
            }
        }
    }

    public void DoSomething()
    {
        Console.WriteLine("Singleton instance is doing something...");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Singleton s1 = Singleton.Instance;
        s1.DoSomething();

        Singleton s2 = Singleton.Instance;
        s2.DoSomething();

        if (s1 == s2)
        {
            Console.WriteLine("s1 and s2 refer to the same Singleton instance");
        }
    }
}
