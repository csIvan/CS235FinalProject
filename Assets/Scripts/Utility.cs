using System.Linq;

public static class Utility
{
    private static System.Random rng = new System.Random();

    public static int[] randomIntArray(int length)
    {
        // Generate an array containing each index of the combinations array
        int[] randomInts = Enumerable.Range(0, length).ToArray();

        // Randomize the array using Fisher-Yates shuffle
        for (int index1 = randomInts.Length - 1; index1 > 0; index1--)
        {
            index1--;
            int index2 = rng.Next(index1 + 1);
            int tempValue = randomInts[index2];
            randomInts[index2] = randomInts[index1];
            randomInts[index1] = tempValue;
        }

        return randomInts;
    }
}
