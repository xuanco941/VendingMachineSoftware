namespace StyleX.Utils
{
    public class Utils
    {
        public static int[] GenerateRandomArray(int minValue, int maxValue, int length)
        {
            int[] result = new int[length];
            Random random = new Random();
            int index = 0;

            while (index < length)
            {
                int randomNumber = random.Next(minValue, maxValue + 1);

                if (Array.IndexOf(result, randomNumber) == -1)
                {
                    result[index] = randomNumber;
                    index++;
                }
            }

            return result;
        }
    }
}
