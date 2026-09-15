using System;

public class Workshop
{
    public string Name { get; }
    public int Capacity { get; }
    public int Registered { get; private set; }

    public int SpacesRemaining
    {
        get { return Capacity - Registered; }
    }

    public Workshop(string name, int capacity)
    {
        Name = name;
        Capacity = capacity;
        Registered = 0;
    }

    public bool RegisterGroup(int groupSize)
    {
        if (groupSize < 0)
        {
            return false;
        }

        if (Registered + groupSize < Capacity)
        {
            Registered += groupSize;
            return true;
        }

        return false;
    }

    public void DisplayStatus()
    {
        Console.WriteLine($"{Name}: {Registered}/{Capacity} registered");
        Console.WriteLine($"{SpacesRemaining} spaces remaining");
    }
}

public class Program
{
    public static void Main()
    {
        Workshop workshop =
            new Workshop("Interactive Storytelling", 12);

        int[] groupRequests = { 4, 3, 5, 1, 0, -2 };

        workshop.DisplayStatus();

        foreach (int groupSize in groupRequests)
        {
            bool registered = workshop.RegisterGroup(groupSize);

            Console.WriteLine(
                $"Request for {groupSize}: {registered}"
            );
        }

        workshop.DisplayStatus();
    }
}