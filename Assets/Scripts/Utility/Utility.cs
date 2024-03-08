public static class Utility
{
    public static int CeilToNearestPowerOf2(int value)
    {
        int target = 2;
        while (target < value)
            target <<= 1;
        return target;
    }
}
