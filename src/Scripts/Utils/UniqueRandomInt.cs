using System;
using System.Collections.Generic;

/// <summary>
/// This class is a unique random number generator.
/// Use <see cref="UniqueRandomInt.SetRange(int, int)"/> to set the range.
/// Use <see cref="UniqueRandomInt.Clear"/> to clear the range.
/// Use <see cref="UniqueRandomInt.RestoreRange"/> to reset the generation mechanism with the same range so you can use it again
/// </summary>
public class UniqueRandomInt
{
    private List<int> m_AllNumber = new List<int>();
    private List<int> m_Generated = new List<int>();
    Random m_Rand = new Random();

    /// <summary>
    /// Set the range for this unique random number generator
    /// </summary>
    /// <param name="min">min, inclusive</param>
    /// <param name="max">max, exclusive</param>
    public void SetRange(int min, int max)
    {
        if (max <= min)
        {
            throw new Exception("UniqueRandomInt: max < min!");
        }

        for (int i = min; i < max; ++i)
        {
            m_AllNumber.Add(i);
        }
    }

    /// <summary>
    /// Get a unique random number from the range specified
    /// </summary>
    /// <returns>random number</returns>
    public int GetUniqueRand()
    {
        int index = m_Rand.Next(m_AllNumber.Count);
        int randVal = m_AllNumber[index];
        m_AllNumber.Remove(index);
        m_Generated.Add(randVal);
        return randVal;
    }

    /// <summary>
    /// Clear everything of the RNG. Need to call <see cref="UniqueRandomInt.SetRange(int, int)"/> before using it again
    /// </summary>
    public void Clear()
    {
        m_AllNumber.Clear();
        m_Generated.Clear();
    }

    /// <summary>
    /// Restore generated number so you can use it again as fresh and keeping the original range
    /// </summary>
    public void RestoreRange()
    {
        m_AllNumber.AddRange(m_Generated);
        m_Generated.Clear();
    }
}
