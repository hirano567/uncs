# uncs
Unofficial C# Compiler for personal experimentations

A C# compiler for personal experimentations. It is derived from sscli 2.0.
This is extended to use the expression trees of .NET Framework 4.

I rewrote the C# compiler of sscli 2.0 in C# for understanding from a few years ago,
and I implemented C# 3.0 extensions. It is published as "Elementary C# Compiler" in CodePlex.
I am implementing the C# 4 extensions to this project now.
When I implemented the expression trees, I was tempted to try to extend the compiler to use the expression trees of .NET Framework 4 and have started this project to implement some experimental extensions.

This compiler can translate several statement lambdas to the expression trees. For example, it can compile the code below:
    using System;
    using System.Linq.Expressions;
    
    namespace ExpressionTree
    {
        class Program
        {
            static void Main(string[] args)
            {
                Expression<Func<int, int>> et = i =>
                {
                    int x = 0;
                    for (; i > 0; --i)
                    {
                        x += i;
                    }
                    return x;
                };
                Func<int, int> fn = et.Compile();
    
                Console.WriteLine("{0}", fn(10));
            }
        }
    }

But, its translation is inadequate and its error processing is very poor. I would like to correct these problems and also to fix the bugs. If you have any advices, tell me please.

This project is developed on .NET Framework 4.6.1 with Visual Studio 2015 Community.
