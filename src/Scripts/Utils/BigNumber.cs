using System;

[Serializable]
public struct BigNumber
{
    /*
        Unity float (System.Single) range:  ±1.5E−45 to ±3.4E38
        Precision: ~6-9 digits

        Yottabyte is 1E24 bytes, after that we add more char to it. For example, MYB, TYB, GYYB, etc.

        We keep the base float to no more than 9.99E3 to preserve accuracy + have a good visual.
        (If it's greater than 1E7 .NET rounds it)
        For a + b or a - b, if the diff between a and b is greater than the magnitude of 1E9, ignore it.

        For example, use 9 * 60 (fps) * 3600 (second per hour) * 24 (hours per day) * 1E-9 = ~0.045. (4.5%)
        That's the accumalative difference between a and b assuming an operation is performed each frame for a consecutive 24 hours 60fps.

        Value behind the representation: (m_Base)*E(m_Power)

        For casting string representation back to BN, we start with BN then get all letters to represent the base.
        Example: BN24.55MY = 24.55E(6+24) = 24.55E30
     */

    private float m_Base;
    private int m_Power;
    /// <summary>
    /// A string with its quick value to display
    /// </summary>
    private string m_Display;

    // Properties
    public float Base
    {
        get
        {
            return m_Base;
        }
    }
    public int Power
    {
        get
        {
            return m_Power;
        }
    }

    // Public methods
    public BigNumber(float number)
    {
        m_Base = number;
        m_Power = 0;
        m_Display = "";
        UpdateDisplay();
    }

    public BigNumber(BigNumber number)
    {
        this.m_Base = number.m_Base;
        this.m_Power = number.m_Power;
        this.m_Display = number.m_Display;
    }

    public void SetValue(float f)
    {
        m_Base = f;
        m_Power = 0;
        m_Display = "";
        UpdateDisplay();
    }

    public void Exp(float power)
    {
        m_Base *= UnityEngine.Mathf.Exp(power);
        UpdateDisplay();
    }

    public void AssignStringValue(string value)
    {
        if (value.IndexOf("BN") < 0)
        {
            bool canConvert = float.TryParse(value, out float fValue);
            if (canConvert)
            {
                this = new BigNumber(fValue);
            }
            else
                throw new Exception("Wrong string value assgined! : " + value);
        }
        else
        {
            string realValue = value.Substring(2);
            // Start from the rear, add base first
            for (int i = realValue.Length - 1; i >= 0; --i)
            {
                if (Char.IsLetter(realValue[i]))
                {
                    switch (realValue[i])
                    {

                        case 'K':
                            {
                                m_Power += 3;
                                break;
                            }
                        case 'M':
                            {
                                m_Power += 6;
                                break;
                            }
                        case 'G':
                            {
                                m_Power += 9;
                                break;
                            }
                        case 'T':
                            {
                                m_Power += 12;
                                break;
                            }
                        case 'P':
                            {
                                m_Power += 15;
                                break;
                            }
                        case 'E':
                            {
                                m_Power += 18;
                                break;
                            }
                        case 'Z':
                            {
                                m_Power += 21;
                                break;
                            }
                        case 'Y':
                            {
                                m_Power += 24;
                                break;
                            }
                        default:
                            {
                                throw new Exception("BigNumber: wrong power char from string! " + realValue[i]);
                            }
                    }
                }
                else
                {
                    string restFloatValue = realValue.Substring(0, i + 1);
                    bool canParse = float.TryParse(restFloatValue, out m_Base);
                    if (!canParse)
                        throw new Exception("BigNumber: unable to parse float from string input! " + restFloatValue);

                    UpdateDisplay();
                    break;
                }
            }
        }
    }

    public void Add(BigNumber b)
    {
        if (this.m_Power >= b.m_Power)
        {
            if (this.m_Power > b.m_Power + 9)
                return;
            else
            {
                float bBase = b.m_Base;
                int bPower = b.m_Power;
                while (this.m_Power > bPower)
                {
                    bPower += 3;
                    bBase /= 1000f;
                }
                this.m_Base += bBase;
                this.UpdateDisplay();
            }
        }
        else if (b.m_Power > this.m_Power)
        {
            if (b.m_Power > this.m_Power + 9)
            {
                this.m_Power = b.m_Power;
                this.m_Base = b.m_Base;
                this.UpdateDisplay();
            }
            else
            {
                while (b.m_Power > this.m_Power)
                {
                    this.m_Power += 3;
                    this.m_Base /= 1000f;
                }
                this.m_Base += b.m_Base;
                UpdateDisplay();
            }
        }
        else
            throw new Exception("BigNumber: + operator wrong");
    }

    public void Add(float b)
    {
        BigNumber newB = new BigNumber(b);
        this.Add(newB);
    }

    public void Minus(BigNumber b)
    {
        if (this.m_Power <= b.m_Power)
        {
            if (b.m_Power - this.m_Power > 9)
            {
                this.m_Base = -b.m_Base;
                this.m_Power = b.m_Power;
                UpdateDisplay();
            }
            else
            {
                while (this.m_Power < b.m_Power)
                {
                    this.m_Power += 3;
                    this.m_Base /= 1000f;
                }
                this.m_Base -= b.m_Base;
                UpdateDisplay();
            }
        }
        else if (this.m_Power > b.m_Power)
        {
            if (this.m_Power > b.m_Power + 9)
            {
                return;
            }
            float bBase = b.m_Base;
            int bPower = b.m_Power;
            while (this.m_Power > bPower)
            {
                bPower += 3;
                bBase /= 1000f;
            }
            this.m_Base -= bBase;
            UpdateDisplay();
        }
        else
            throw new Exception("BigNumber: - operator wrong");
    }

    public void Minus(float b)
    {
        BigNumber newB = new BigNumber(b);
        this.Minus(newB);
    }

    public void Multiply(BigNumber b)
    {
        this.m_Base *= b.m_Base;
        this.m_Power += b.m_Power;
        UpdateDisplay();
    }

    public void Multiply(float b)
    {
        BigNumber big = new BigNumber(b);
        this.Multiply(big);
    }

    //public void Multyply(int b)
    //{
    //    this.Multiply((float)b);
    //}

    public void Div(BigNumber b)
    {
        if (b.m_Base == 0f)
        {
            throw new DivideByZeroException("BigNumber - divide by zero!");
        }
        this.m_Base /= b.m_Base;
        this.m_Power -= b.m_Power;
        UpdateDisplay();
    }

    public void Div(float b)
    {
        BigNumber big = new BigNumber(b);
        this.Div(big);
    }

    public float GetRatioDivBy(BigNumber b)
    {
        float fBase = this.m_Base / b.m_Base;
        return fBase * (float) Math.Pow(10, this.m_Power - b.m_Power);
    }

    public override string ToString()
    {
        return m_Display;
    }

    /// <summary>
    /// Compare with another BigNumber
    /// </summary>
    /// <param name="b">The other big number to be compared</param>
    /// <returns>"1" if this is larger, "-1" for smaller, "0" if equals each other. E.G. for "<=", use "!= 1"</returns>
    public int Compare(BigNumber b)
    {
        ////TODO: change all return value from direct int to const
        if (this.m_Base >= 0 && b.m_Base < 0)
        {
            return 1;
        }
        else if (this.m_Base <= 0 && b.m_Base > 0)
        {
            return -1;
        }
        else if (this.m_Base >= 0 && b.m_Base >= 0)
        {
            if (this.m_Power > b.m_Power)
            {
                return 1;
            }
            else if (this.m_Power < b.m_Power)
            {
                return -1;
            }
            else
            {
                if (this.m_Base > b.m_Base)
                {
                    return 1;
                }
                else if (this.m_Base < b.m_Base)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
        else if (this.m_Base <= 0 && b.m_Base <= 0)
        {
            if (this.m_Power > b.m_Power)
            {
                return -1;
            }
            else if (this.m_Power < b.m_Power)
            {
                return 1;
            }
            else
            {
                if (this.m_Base > b.m_Base)
                {
                    return 1;
                }
                else if (this.m_Base < b.m_Base)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
        else
        {
            throw new Exception("Somthing went wrong when comparing big numbers: " + this.ToString() + " and " + b.ToString());
        }
    }

    public int Compare(float b)
    {
        return Compare(new BigNumber(b));
    }

    // Private methods
    private void UpdateDisplay()
    {
        while (Math.Abs(m_Base) < 1f && m_Power >= 3)
        {
            m_Base *= 1000f;
            m_Power -= 3;
        }

        while (Math.Abs(m_Base) > 999f || m_Power < 0)
        {
            m_Base /= 1000f;
            m_Power += 3;
        }

        string suffix = "";
        int tempPower = m_Power;

        float tempBase = m_Base;
        while (Math.Abs(tempBase) > 1000f)
        {
            tempBase /= 1000f;
            tempPower += 3;
        }

        while (Math.Abs(tempBase) < 1f && tempPower > 0)
        {
            tempBase *= 1000f;
            tempPower -= 3;
        }

        while (tempPower >= 24)
        {
            suffix += "Y";
            tempPower -= 24;
        }

        switch (tempPower)
        {
            case 0:
                break;
            case 3:
                suffix = "K" + suffix;
                break;
            case 6:
                suffix = "M" + suffix;
                break;
            case 9:
                suffix = "G" + suffix;
                break;
            case 12:
                suffix = "T" + suffix;
                break;
            case 15:
                suffix = "P" + suffix;
                break;
            case 18:
                suffix = "E" + suffix;
                break;
            case 21:
                suffix = "Z" + suffix;
                break;
            default:
                throw new Exception("BigNumber: power of 10 not !");
        }
        m_Display = tempBase.ToString("f3") + suffix;
    }

    public bool GreaterThanZero()
    {
        return m_Base > 0f;
    }

    public bool EqualsZero()
    {
        return m_Base == 0;
    }

    public bool LessThanZero()
    {
        return m_Base < 0f;
    }

    public void ResetToZero()
    {
        m_Base = 0f;
        m_Power = 0;
        UpdateDisplay();
    }

    public void Round()
    {
        if (m_Power == 0)
        {
            m_Base = UnityEngine.Mathf.Round(m_Base);
            UpdateDisplay();
        }
    }
}
