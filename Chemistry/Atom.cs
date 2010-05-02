using System;

namespace Chemistry
{
    public struct Atom : IEquatable<Atom>
    {
        private byte p, n, e;

        public Atom(byte p, byte n, byte e)
        {
            this.p = p;
            this.n = n;
            this.e = e;
        }

        public Atom(byte p, byte n)
            : this(p, n, p) { }

        public Element Element
        {
            get { return (Element)p; }
        }

        public int MassNumber
        {
            get { return p + n; }
        }

        public bool IsIon
        {
            get { return p != e; }
        }

        public int Charge
        {
            get { return p - e; }
        }

        public bool Equals(Atom other)
        {
            return p == other.p && n == other.n && e == other.e;
        }

        public static bool operator ==(Atom a1, Atom a2)
        { return a1.Equals(a2); }

        public static bool operator !=(Atom a1, Atom a2)
        { return !a1.Equals(a2); }

        public override string ToString()
        {
            string s = MassNumber + Element.ToString();
            if (IsIon) s += Math.Abs(Charge) + (Charge > 0 ? "+" : "-");
            return s;
        }

        public override int GetHashCode()
        {
            return p * 65536 + n * 256 + e;
        }
    }
}