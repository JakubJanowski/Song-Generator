using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    [Serializable]
    public enum Phoneme : byte
    {
        AA, // odd = AA D (vowel)
        AE, // at = AE T (vowel)
        AH, // hut = HH AH T (vowel)
        AO, // ought = AO T (vowel)
        AW, // cow = K AW (vowel)
        AY, // hide = HH AY D (vowel)
        B,  // be = B IY
        CH, // cheese = CH IY Z
        D,  // dee = D IY
        DH, // thee = DH IY
        EH, // Ed = EH D (vowel)
        ER, // hurt = HH ER T (vowel)
        EY, // ate = EY T (vowel)
        F,  // fee = F IY
        G,  // green = G R IY N
        HH, // he = HH IY
        IH, // it = IH T (vowel)
        IY, // eat = IY T (vowel)
        JH, // gee = JH IY
        K,  // key = K IY
        L,  // lee = L IY
        M,  // me = M IY
        N,  // knee = N IY
        NG, // ping = P IH NG
        OW, // oat = OW T (vowel)
        OY, // toy = T OY (vowel)
        P,  // pee = P IY
        R,  // read = R IY D
        S,  // sea = S IY
        SH, // she = SH IY
        T,  // tea = T IY
        TH, // theta = TH EY T AH
        UH, // hood = HH UH D (vowel)
        UW, // two = T UW (vowel)
        V,  // vee = V IY
        W,  // we = W IY
        Y,  // yield = Y IY L D
        Z,  // zee = Z IY
        ZH  // seizure = S IY ZH ER
    }

    [Serializable]
    public enum Accent : byte
    {
        NoStress = 0,
        Primary = 1,
        Secondary = 2,
        Consonant = 4
    }

    [Serializable]
    public struct AccentedPhoneme
    {
        public Phoneme phoneme;
        public Accent accent;

        private static Phoneme StringToPhoneme(string s)
        {
            switch (s[0])
            {
                case 'A':
                    switch (s[1])
                    {
                        case 'A':
                            return Phoneme.AA;
                        case 'E':
                            return Phoneme.AE;
                        case 'H':
                            return Phoneme.AH;
                        case 'O':
                            return Phoneme.AO;
                        case 'W':
                            return Phoneme.AW;
                        default:
                            return Phoneme.AY;
                    }
                case 'B':
                    return Phoneme.B;
                case 'C':
                    return Phoneme.CH;
                case 'D':
                    switch (s.Length)
                    {
                        case 1:
                            return Phoneme.D;
                        default:
                            return Phoneme.DH;
                    }
                case 'E':
                    switch (s[1])
                    {
                        case 'H':
                            return Phoneme.EH;
                        case 'R':
                            return Phoneme.ER;
                        default:
                            return Phoneme.EY;
                    }
                case 'F':
                    return Phoneme.F;
                case 'G':
                    return Phoneme.G;
                case 'H':
                    return Phoneme.HH;
                case 'I':
                    switch (s[1])
                    {
                        case 'H':
                            return Phoneme.IH;
                        default:
                            return Phoneme.IY;
                    }
                case 'J':
                    return Phoneme.JH;
                case 'K':
                    return Phoneme.K;
                case 'L':
                    return Phoneme.L;
                case 'M':
                    return Phoneme.M;
                case 'N':
                    switch (s.Length)
                    {
                        case 1:
                            return Phoneme.N;
                        default:
                            return Phoneme.NG;
                    }
                case 'O':
                    switch (s[1])
                    {
                        case 'W':
                            return Phoneme.OW;
                        default:
                            return Phoneme.OY;
                    }
                case 'P':
                    return Phoneme.P;
                case 'R':
                    return Phoneme.R;
                case 'S':
                    switch (s.Length)
                    {
                        case 1:
                            return Phoneme.S;
                        default:
                            return Phoneme.SH;
                    }
                case 'T':
                    switch (s.Length)
                    {
                        case 1:
                            return Phoneme.T;
                        default:
                            return Phoneme.TH;
                    }
                case 'U':
                    switch (s[1])
                    {
                        case 'H':
                            return Phoneme.UH;
                        default:
                            return Phoneme.UW;
                    }
                case 'V':
                    return Phoneme.V;
                case 'W':
                    return Phoneme.W;
                case 'Y':
                    return Phoneme.Y;
                default:
                    switch (s.Length)
                    {
                        case 1:
                            return Phoneme.Z;
                        default:
                            return Phoneme.ZH;
                    }
            }
        }

        public AccentedPhoneme(string str)
        {
            // create a new stressed phoneme object from its string representation

            // check for a final digit (indicates a vowel)
            string phonemeCode;
            int finalIndex = str.Length - 1;
            if ((str[finalIndex] == '0') || (str[finalIndex] == '1') || (str[finalIndex] == '2'))
            {
                this.accent = (Accent)Int32.Parse(Convert.ToString(str[finalIndex]));
                phonemeCode = str.Substring(0, finalIndex);
            }
            else
            {
                this.accent = Accent.Consonant;
                phonemeCode = str;
            }

            // use the rest of the string to form the phoneme
            //this.phoneme = (Phoneme)Enum.Parse(typeof(Phoneme), phonemecode);
            this.phoneme = StringToPhoneme(phonemeCode);
        }

        public override string ToString()
        {
            return phoneme.ToString() + "-" + accent.ToString();
        }
    }
}
