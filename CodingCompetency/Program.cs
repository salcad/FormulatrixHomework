using System;

namespace CodingCompetency
{
    internal class Program
    {
        static void Main()
        {
            QuizSolution();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        static void QuizSolution()
        {
            // Test for Question 1
            NumberGenerator gen1 = new NumberGenerator();
            gen1.AddRule(3, "foo");
            gen1.AddRule(5, "bar");
            gen1.AddRule(15, "foobar");
            Console.WriteLine("Question 1 output for n=15:");
            gen1.PrintSequence(15);

            // Test for Question 2
            NumberGenerator gen2 = new NumberGenerator();
            gen2.AddRule(3, "foo");
            gen2.AddRule(5, "bar");
            gen2.AddRule(7, "jazz");
            gen2.AddRule(15, "foobar");
            gen2.AddRule(21, "foojazz");
            gen2.AddRule(35, "barjazz");
            gen2.AddRule(105, "foobarjazz");
            Console.WriteLine("\nQuestion 2 output for n=105:");
            gen2.PrintSequence(105);

            // Test for Question 3
            NumberGenerator gen3 = new NumberGenerator();
            gen3.AddRule(3, "foo");
            gen3.AddRule(4, "baz");
            gen3.AddRule(5, "bar");
            gen3.AddRule(7, "jazz");
            gen3.AddRule(9, "huzz");
            Console.WriteLine("\nQuestion 3 output for n=20:");
            gen3.PrintSequence(20);

            // Test for Question 4 (already implemented as class with AddRule API)
            NumberGenerator gen4 = new NumberGenerator();
            // Example of custom rules
            gen4.AddRule(2, "even");
            gen4.AddRule(3, "triple");
            Console.WriteLine("\nQuestion 4 output with custom rules for n=10:");
            gen4.PrintSequence(10);
        }
    }
}
