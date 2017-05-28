/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace OpenSim.Region.ScriptEngine.Common
{
    [Serializable]
    public partial class LSL_Types
    {
        // Types are kept is separate .dll to avoid having to add whatever .dll it is in it to script AppDomain

        [Serializable]
        public struct Vector3
        {
            public double x;
            public double y;
            public double z;

            #region Constructors

            public Vector3(Vector3 vector)
            {
                x = (float)vector.x;
                y = (float)vector.y;
                z = (float)vector.z;
            }

            public Vector3(double X, double Y, double Z)
            {
                x = X;
                y = Y;
                z = Z;
            }

            public Vector3(string str)
            {
                str = str.Replace('<', ' ');
                str = str.Replace('>', ' ');
                string[] tmps = str.Split(new Char[] { ',', '<', '>' });
                if (tmps.Length < 3)
                {
                    x=y=z=0;
                    return;
                }
                bool res;
                res = Double.TryParse(tmps[0], out x);
                res = res & Double.TryParse(tmps[1], out y);
                res = res & Double.TryParse(tmps[2], out z);
            }

            #endregion

            #region Overriders

            public override string ToString()
            {
                string s=String.Format("<{0:0.000000},{1:0.000000},{2:0.000000}>", x, y, z);
                return s;
            }

            public static explicit operator LSLString(Vector3 vec)
            {
                string s=String.Format("<{0:0.000000},{1:0.000000},{2:0.000000}>", vec.x, vec.y, vec.z);
                return new LSLString(s);
            }

            public static explicit operator string(Vector3 vec)
            {
                string s=String.Format("<{0:0.000000},{1:0.000000},{2:0.000000}>", vec.x, vec.y, vec.z);
                return s;
            }

            public static explicit operator Vector3(string s)
            {
                return new Vector3(s);
            }

            public static bool operator ==(Vector3 lhs, Vector3 rhs)
            {
                return (lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z);
            }

            public static bool operator !=(Vector3 lhs, Vector3 rhs)
            {
                return !(lhs == rhs);
            }

            public override int GetHashCode()
            {
                return (x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode());
            }

            public override bool Equals(object o)
            {
                if (!(o is Vector3)) return false;

                Vector3 vector = (Vector3)o;

                return (x == vector.x && x == vector.x && z == vector.z);
            }

            #endregion

            #region Vector & Vector Math

            // Vector-Vector Math
            public static Vector3 operator +(Vector3 lhs, Vector3 rhs)
            {
                return new Vector3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
            }

            public static Vector3 operator -(Vector3 lhs, Vector3 rhs)
            {
                return new Vector3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
            }

            public static Vector3 operator *(Vector3 lhs, Vector3 rhs)
            {
                return new Vector3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
            }

            public static Vector3 operator %(Vector3 v1, Vector3 v2)
            {
                //Cross product
                Vector3 tv;
                tv.x = (v1.y * v2.z) - (v1.z * v2.y);
                tv.y = (v1.z * v2.x) - (v1.x * v2.z);
                tv.z = (v1.x * v2.y) - (v1.y * v2.x);
                return tv;
            }

            #endregion

            #region Vector & Float Math

            // Vector-Float and Float-Vector Math
            public static Vector3 operator *(Vector3 vec, float val)
            {
                return new Vector3(vec.x * val, vec.y * val, vec.z * val);
            }

            public static Vector3 operator *(float val, Vector3 vec)
            {
                return new Vector3(vec.x * val, vec.y * val, vec.z * val);
            }

            public static Vector3 operator /(Vector3 v, float f)
            {
                v.x = v.x / f;
                v.y = v.y / f;
                v.z = v.z / f;
                return v;
            }

            #endregion

            #region Vector & Double Math

            public static Vector3 operator *(Vector3 vec, double val)
            {
                return new Vector3(vec.x * val, vec.y * val, vec.z * val);
            }

            public static Vector3 operator *(double val, Vector3 vec)
            {
                return new Vector3(vec.x * val, vec.y * val, vec.z * val);
            }

            public static Vector3 operator /(Vector3 v, double f)
            {
                v.x = v.x / f;
                v.y = v.y / f;
                v.z = v.z / f;
                return v;
            }

            #endregion

            #region Vector & Rotation Math

            // Vector-Rotation Math
            public static Vector3 operator *(Vector3 v, Quaternion r)
            {
                Quaternion vq = new Quaternion(v.x, v.y, v.z, 0);
                Quaternion nq = new Quaternion(-r.x, -r.y, -r.z, r.s);

                // adapted for operator * computing "b * a"
                Quaternion result = nq * (vq * r);

                return new Vector3(result.x, result.y, result.z);
            }

            public static Vector3 operator /(Vector3 v, Quaternion r)
            {
                r.s = -r.s;
                return v * r;
            }

            #endregion

            #region Static Helper Functions

            public static double Dot(Vector3 v1, Vector3 v2)
            {
                return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
            }

            public static Vector3 Cross(Vector3 v1, Vector3 v2)
            {
                return new Vector3
                    (
                    v1.y * v2.z - v1.z * v2.y,
                    v1.z * v2.x - v1.x * v2.z,
                    v1.x * v2.y - v1.y * v2.x
                    );
            }

            public static double Mag(Vector3 v)
            {
                return Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            }

            public static Vector3 Norm(Vector3 vector)
            {
                double mag = Mag(vector);
                return new Vector3(vector.x / mag, vector.y / mag, vector.z / mag);
            }

            #endregion
        }

        [Serializable]
        public struct Quaternion
        {
            public double x;
            public double y;
            public double z;
            public double s;

            #region Constructors

            public Quaternion(Quaternion Quat)
            {
                x = (float)Quat.x;
                y = (float)Quat.y;
                z = (float)Quat.z;
                s = (float)Quat.s;
                if (x == 0 && y == 0 && z == 0 && s == 0)
                    s = 1;
            }

            public Quaternion(double X, double Y, double Z, double S)
            {
                x = X;
                y = Y;
                z = Z;
                s = S;
                if (x == 0 && y == 0 && z == 0 && s == 0)
                    s = 1;
            }

            public Quaternion(string str)
            {
                str = str.Replace('<', ' ');
                str = str.Replace('>', ' ');
                string[] tmps = str.Split(new Char[] { ',', '<', '>' });
                if (tmps.Length < 4)
                {
                    x=y=z=s=0;
                    return;
                }
                bool res;
                res = Double.TryParse(tmps[0], out x);
                res = res & Double.TryParse(tmps[1], out y);
                res = res & Double.TryParse(tmps[2], out z);
                res = res & Double.TryParse(tmps[3], out s);
                if (x == 0 && y == 0 && z == 0 && s == 0)
                    s = 1;
            }

            #endregion

            #region Overriders

            public override int GetHashCode()
            {
                return (x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ s.GetHashCode());
            }

            public override bool Equals(object o)
            {
                if (!(o is Quaternion)) return false;

                Quaternion quaternion = (Quaternion)o;

                return x == quaternion.x && y == quaternion.y && z == quaternion.z && s == quaternion.s;
            }

            public override string ToString()
            {
                string st=String.Format("<{0:0.000000},{1:0.000000},{2:0.000000},{3:0.000000}>", x, y, z, s);
                return st;
            }

            public static explicit operator string(Quaternion r)
            {
                string s=String.Format("<{0:0.000000},{1:0.000000},{2:0.000000},{3:0.000000}>", r.x, r.y, r.z, r.s);
                return s;
            }

            public static explicit operator LSLString(Quaternion r)
            {
                string s=String.Format("<{0:0.000000},{1:0.000000},{2:0.000000},{3:0.000000}>", r.x, r.y, r.z, r.s);
                return new LSLString(s);
            }

            public static explicit operator Quaternion(string s)
            {
                return new Quaternion(s);
            }

            public static bool operator ==(Quaternion lhs, Quaternion rhs)
            {
                // Return true if the fields match:
                return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.s == rhs.s;
            }

            public static bool operator !=(Quaternion lhs, Quaternion rhs)
            {
                return !(lhs == rhs);
            }

            #endregion

            public static Quaternion operator +(Quaternion a, Quaternion b)
            {
                return new Quaternion(a.x + b.x, a.y + b.y, a.z + b.z, a.s + b.s);
            }

            public static Quaternion operator /(Quaternion a, Quaternion b)
            {
                b.s = -b.s;
                return a * b;
            }

            public static Quaternion operator -(Quaternion a, Quaternion b)
            {
                return new Quaternion(a.x - b.x, a.y - b.y, a.z - b.z, a.s - b.s);
            }

            // using the equations below, we need to do "b * a" to be compatible with LSL
            public static Quaternion operator *(Quaternion b, Quaternion a)
            {
                Quaternion c;
                c.x = a.s * b.x + a.x * b.s + a.y * b.z - a.z * b.y;
                c.y = a.s * b.y + a.y * b.s + a.z * b.x - a.x * b.z;
                c.z = a.s * b.z + a.z * b.s + a.x * b.y - a.y * b.x;
                c.s = a.s * b.s - a.x * b.x - a.y * b.y - a.z * b.z;
                return c;
            }
        }

        [Serializable]
        public class list
        {
            private object[] m_data;

            public list(params object[] args)
            {
                m_data = new object[args.Length];
                m_data = args;
            }

            public int Length
            {
                get {
                    if (m_data == null)
                        m_data=new Object[0];
                    return m_data.Length;
                }
            }

            public object[] Data
            {
                get {
                    if (m_data == null)
                        m_data=new Object[0];
                    return m_data;
                }
            }

            public static list operator +(list a, list b)
            {
                object[] tmp;
                tmp = new object[a.Length + b.Length];
                a.Data.CopyTo(tmp, 0);
                b.Data.CopyTo(tmp, a.Length);
                return new list(tmp);
            }

            private void ExtendAndAdd(object o)
            {
                Array.Resize(ref m_data, Length + 1);
                m_data.SetValue(o, Length - 1);
            }

            public static list operator +(list a, string s)
            {
                a.ExtendAndAdd(s);
                return a;
            }

            public static list operator +(list a, int i)
            {
                a.ExtendAndAdd(i);
                return a;
            }

            public static list operator +(list a, double d)
            {
                a.ExtendAndAdd(d);
                return a;
            }

            public void Add(object o)
            {
                object[] tmp;
                tmp = new object[m_data.Length + 1];
                m_data.CopyTo(tmp, 0);
                tmp[m_data.Length] = o;
                m_data = tmp;
            }

            public bool Contains(object o)
            {
                bool ret = false;
                foreach (object i in Data)
                {
                    if (i == o)
                    {
                        ret = true;
                        break;
                    }
                }
                return ret;
            }

            public list DeleteSublist(int start, int end)
            {
                // Not an easy one
                // If start <= end, remove that part
                // if either is negative, count from the end of the array
                // if the resulting start > end, remove all BUT that part

                Object[] ret;

                if (start < 0)
                    start=m_data.Length-start;

                if (start < 0)
                    start=0;

                if (end < 0)
                    end=m_data.Length-end;
                if (end < 0)
                    end=0;

                if (start > end)
                {
                    if (end >= m_data.Length)
                        return new list(new Object[0]);

                    if (start >= m_data.Length)
                        start=m_data.Length-1;

                    return GetSublist(end, start);
                }

                // start >= 0 && end >= 0 here
                if (start >= m_data.Length)
                {
                    ret=new Object[m_data.Length];
                    Array.Copy(m_data, 0, ret, 0, m_data.Length);

                    return new list(ret);
                }

                if (end >= m_data.Length)
                    end=m_data.Length-1;

                // now, this makes the math easier
                int remove=end+1-start;

                ret=new Object[m_data.Length-remove];
                if (ret.Length == 0)
                    return new list(ret);

                int src;
                int dest=0;

                for (src = 0; src < m_data.Length; src++)
                {
                    if (src < start || src > end)
                        ret[dest++]=m_data[src];
                }

                return new list(ret);
            }

            public list GetSublist(int start, int end)
            {

                object[] ret;

                // Take care of neg start or end's
                // NOTE that either index may still be negative after
                // adding the length, so we must take additional
                // measures to protect against this. Note also that
                // after normalisation the negative indices are no
                // longer relative to the end of the list.

                if (start < 0)
                {
                    start = m_data.Length + start;
                }

                if (end < 0)
                {
                    end = m_data.Length + end;
                }

                // The conventional case is start <= end
                // NOTE that the case of an empty list is
                // dealt with by the initial test. Start
                // less than end is taken to be the most
                // common case.

                if (start <= end)
                {

                    // Start sublist beyond length
                    // Also deals with start AND end still negative
                    if (start >= m_data.Length || end < 0)
                    {
                        return new list();
                    }

                    // Sublist extends beyond the end of the supplied list
                    if (end >= m_data.Length)
                    {
                        end = m_data.Length - 1;
                    }

                    // Sublist still starts before the beginning of the list
                    if (start < 0)
                    {
                        start = 0;
                    }

                    ret = new object[end - start + 1];

                    Array.Copy(m_data, start, ret, 0, end - start + 1);

                    return new list(ret);

                }

                // Deal with the segmented case: 0->end + start->EOL

                else
                {

                    list result = null;

                    // If end is negative, then prefix list is empty
                    if (end < 0)
                    {
                        result = new list();
                        // If start is still negative, then the whole of
                        // the existing list is returned. This case is
                        // only admitted if end is also still negative.
                        if (start < 0)
                        {
                            return this;
                        }

                    }
                    else
                    {
                        result = GetSublist(0,end);
                    }

                    // If start is outside of list, then just return
                    // the prefix, whatever it is.
                    if (start >= m_data.Length)
                    {
                        return result;
                    }

                    return result + GetSublist(start, Data.Length);

                }
            }

            public list Sort(int stride, int ascending)
            {
                if (Data.Length == 0)
                    return new list(); // Don't even bother

                string[] keys;

                if (stride == 1) // The simple case
                {
                    Object[] ret=new Object[Data.Length];

                    Array.Copy(Data, 0, ret, 0, Data.Length);

                    keys=new string[Data.Length];

                    for (int k = 0; k < Data.Length; k++)
                        keys[k] = Data[k].ToString();

                    Array.Sort(keys, ret);

                    if (ascending == 0)
                        Array.Reverse(ret);
                    return new list(ret);
                }

                int src=0;

                int len=(Data.Length+stride-1)/stride;

                keys=new string[len];
                Object[][] vals=new Object[len][];

                int i;

                while (src < Data.Length)
                {
                    Object[] o=new Object[stride];

                    for (i = 0; i < stride; i++)
                    {
                        if (src < Data.Length)
                            o[i]=Data[src++];
                        else
                        {
                            o[i]=new Object();
                            src++;
                        }
                    }

                    int idx=src/stride-1;
                    keys[idx]=o[0].ToString();
                    vals[idx]=o;
                }

                Array.Sort(keys, vals);
                if (ascending == 0)
                {
                    Array.Reverse(vals);
                }

                Object[] sorted=new Object[stride*vals.Length];

                for (i = 0; i < vals.Length; i++)
                    for (int j = 0; j < stride; j++)
                        sorted[i*stride+j] = vals[i][j];

                return new list(sorted);
            }

            #region CSV Methods

            public static list FromCSV(string csv)
            {
                return new list(csv.Split(','));
            }

            public string ToCSV()
            {
                string ret = "";
                foreach (object o in this.Data)
                {
                    if (ret == "")
                    {
                        ret = o.ToString();
                    }
                    else
                    {
                        ret = ret + ", " + o.ToString();
                    }
                }
                return ret;
            }

            private string ToSoup()
            {
                string output;
                output = String.Empty;
                if (m_data.Length == 0)
                {
                    return String.Empty;
                }
                foreach (object o in m_data)
                {
                    output = output + o.ToString();
                }
                return output;
            }

            public static explicit operator String(list l)
            {
                return l.ToSoup();
            }

            public static explicit operator LSLString(list l)
            {
                return new LSLString(l.ToSoup());
            }

            public override string ToString()
            {
                return ToSoup();
            }

            #endregion

            #region Statistic Methods

            public double Min()
            {
                double minimum = double.PositiveInfinity;
                double entry;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (double.TryParse(Data[i].ToString(), out entry))
                    {
                        if (entry < minimum) minimum = entry;
                    }
                }
                return minimum;
            }

            public double Max()
            {
                double maximum = double.NegativeInfinity;
                double entry;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (double.TryParse(Data[i].ToString(), out entry))
                    {
                        if (entry > maximum) maximum = entry;
                    }
                }
                return maximum;
            }

            public double Range()
            {
                return (this.Max() / this.Min());
            }

            public int NumericLength()
            {
                int count = 0;
                double entry;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (double.TryParse(Data[i].ToString(), out entry))
                    {
                        count++;
                    }
                }
                return count;
            }

            public static list ToDoubleList(list src)
            {
                list ret = new list();
                double entry;
                for (int i = 0; i < src.Data.Length - 1; i++)
                {
                    if (double.TryParse(src.Data[i].ToString(), out entry))
                    {
                        ret.Add(entry);
                    }
                }
                return ret;
            }

            public double Sum()
            {
                double sum = 0;
                double entry;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (double.TryParse(Data[i].ToString(), out entry))
                    {
                        sum = sum + entry;
                    }
                }
                return sum;
            }

            public double SumSqrs()
            {
                double sum = 0;
                double entry;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (double.TryParse(Data[i].ToString(), out entry))
                    {
                        sum = sum + Math.Pow(entry, 2);
                    }
                }
                return sum;
            }

            public double Mean()
            {
                return (this.Sum() / this.NumericLength());
            }

            public void NumericSort()
            {
                IComparer Numeric = new NumericComparer();
                Array.Sort(Data, Numeric);
            }

            public void AlphaSort()
            {
                IComparer Alpha = new AlphaCompare();
                Array.Sort(Data, Alpha);
            }

            public double Median()
            {
                return Qi(0.5);
            }

            public double GeometricMean()
            {
                double ret = 1.0;
                list nums = ToDoubleList(this);
                for (int i = 0; i < nums.Data.Length; i++)
                {
                    ret *= (double)nums.Data[i];
                }
                return Math.Exp(Math.Log(ret) / (double)nums.Data.Length);
            }

            public double HarmonicMean()
            {
                double ret = 0.0;
                list nums = ToDoubleList(this);
                for (int i = 0; i < nums.Data.Length; i++)
                {
                    ret += 1.0 / (double)nums.Data[i];
                }
                return ((double)nums.Data.Length / ret);
            }

            public double Variance()
            {
                double s = 0;
                list num = ToDoubleList(this);
                for (int i = 0; i < num.Data.Length; i++)
                {
                    s += Math.Pow((double)num.Data[i], 2);
                }
                return (s - num.Data.Length * Math.Pow(num.Mean(), 2)) / (num.Data.Length - 1);
            }

            public double StdDev()
            {
                return Math.Sqrt(this.Variance());
            }

            public double Qi(double i)
            {
                list j = this;
                j.NumericSort();

                if (Math.Ceiling(this.Length * i) == this.Length * i)
                {
                    return (double)((double)j.Data[(int)(this.Length * i - 1)] + (double)j.Data[(int)(this.Length * i)]) / 2;
                }
                else
                {
                    return (double)j.Data[((int)(Math.Ceiling(this.Length * i))) - 1];
                }
            }

            #endregion

            public string ToPrettyString()
            {
                string output;
                if (m_data.Length == 0)
                {
                    return "[]";
                }
                output = "[";
                foreach (object o in m_data)
                {
                    if (o is String)
                    {
                        output = output + "\"" + o + "\", ";
                    }
                    else
                    {
                        output = output + o.ToString() + ", ";
                    }
                }
                output = output.Substring(0, output.Length - 2);
                output = output + "]";
                return output;
            }

            public class AlphaCompare : IComparer
            {
                int IComparer.Compare(object x, object y)
                {
                    return string.Compare(x.ToString(), y.ToString());
                }
            }

            public class NumericComparer : IComparer
            {
                int IComparer.Compare(object x, object y)
                {
                    double a;
                    double b;
                    if (!double.TryParse(x.ToString(), out a))
                    {
                        a = 0.0;
                    }
                    if (!double.TryParse(y.ToString(), out b))
                    {
                        b = 0.0;
                    }
                    if (a < b)
                    {
                        return -1;
                    }
                    else if (a == b)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }

        }

        //
        // BELOW IS WORK IN PROGRESS... IT WILL CHANGE, SO DON'T USE YET! :)
        //

        public struct StringTest
        {
            // Our own little string
            internal string actualString;
            public static implicit operator bool(StringTest mString)
            {
                if (mString.actualString.Length == 0)
                    return true;
                return false;
            }
            public override string ToString()
            {
                return actualString;
            }

        }

        [Serializable]
        public struct key
        {
            public string value;

            #region Constructors
            public key(string s)
            {
                value = s;
            }

            #endregion

            #region Methods

            static public bool Parse2Key(string s)
            {
                Regex isuuid = new Regex(@"^[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}$", RegexOptions.Compiled);
                if (isuuid.IsMatch(s))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            #endregion

            #region Operators

            static public implicit operator Boolean(key k)
            {
                if (k.value.Length == 0)
                {
                    return false;
                }

                if (k.value == "00000000-0000-0000-0000-000000000000")
                {
                    return false;
                }
                Regex isuuid = new Regex(@"^[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}$", RegexOptions.Compiled);
                if (isuuid.IsMatch(k.value))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            static public implicit operator key(string s)
            {
                return new key(s);
            }

            static public implicit operator String(key k)
            {
                return k.value;
            }

            public static bool operator ==(key k1, key k2)
            {
                return k1.value == k2.value;
            }
            public static bool operator !=(key k1, key k2)
            {
                return k1.value != k2.value;
            }

            #endregion

            #region Overriders

            public override bool Equals(object o)
            {
                return o.ToString() == value;
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }

            #endregion
        }

        [Serializable]
        public struct LSLString
        {
            public string m_string;
            #region Constructors
            public LSLString(string s)
            {
                m_string = s;
            }

            public LSLString(double d)
            {
                string s=String.Format("{0:0.000000}", d);
                m_string=s;
            }

            public LSLString(LSLFloat f)
            {
                string s=String.Format("{0:0.000000}", f.value);
                m_string=s;
            }

            #endregion

            #region Operators
            static public implicit operator Boolean(LSLString s)
            {
                if (s.m_string.Length == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }



            static public implicit operator String(LSLString s)
            {
                return s.m_string;
            }

            static public implicit operator LSLString(string s)
            {
                return new LSLString(s);
            }

            public static string ToString(LSLString s)
            {
                return s.m_string;
            }

            public override string ToString()
            {
                return m_string;
            }

            public static bool operator ==(LSLString s1, string s2)
            {
                return s1.m_string == s2;
            }

            public static bool operator !=(LSLString s1, string s2)
            {
                return s1.m_string != s2;
            }

            public static explicit operator double(LSLString s)
            {
                return Convert.ToDouble(s.m_string);
            }

            public static explicit operator LSLInteger(LSLString s)
            {
                return new LSLInteger(Convert.ToInt32(s.m_string));
            }

            public static explicit operator LSLString(double d)
            {
                return new LSLString(d);
            }

            public static explicit operator LSLString(LSLFloat f)
            {
                return new LSLString(f);
            }

            public static implicit operator Vector3(LSLString s)
            {
                return new Vector3(s.m_string);
            }

            #endregion

            #region Overriders
            public override bool Equals(object o)
            {
                return m_string == o.ToString();
            }

            public override int GetHashCode()
            {
                return m_string.GetHashCode();
            }

            #endregion

            #region " Standard string functions "
            //Clone,CompareTo,Contains
            //CopyTo,EndsWith,Equals,GetEnumerator,GetHashCode,GetType,GetTypeCode
            //IndexOf,IndexOfAny,Insert,IsNormalized,LastIndexOf,LastIndexOfAny
            //Length,Normalize,PadLeft,PadRight,Remove,Replace,Split,StartsWith,Substring,ToCharArray,ToLowerInvariant
            //ToString,ToUpper,ToUpperInvariant,Trim,TrimEnd,TrimStart
            public bool Contains(string value) { return m_string.Contains(value); }
            public int IndexOf(string value) { return m_string.IndexOf(value); }
            public int Length { get { return m_string.Length; } }


            #endregion
        }

        [Serializable]
        public struct LSLInteger
        {
            public int value;

            #region Constructors
            public LSLInteger(int i)
            {
                value = i;
            }

            public LSLInteger(double d)
            {
                value = (int)d;
            }

            #endregion

            #region Operators

            static public implicit operator int(LSLInteger i)
            {
                return i.value;
            }

            static public implicit operator uint(LSLInteger i)
            {
                return (uint)i.value;
            }

            static public explicit operator LSLString(LSLInteger i)
            {
                return new LSLString(i.ToString());
            }

            static public implicit operator Boolean(LSLInteger i)
            {
                if (i.value == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            static public implicit operator LSLInteger(int i)
            {
                return new LSLInteger(i);
            }

            static public explicit operator LSLInteger(string s)
            {
                return new LSLInteger(int.Parse(s));
            }

            static public implicit operator LSLInteger(double d)
            {
                return new LSLInteger(d);
            }

            static public bool operator ==(LSLInteger i1, LSLInteger i2)
            {
                bool ret = i1.value == i2.value;
                return ret;
            }

            static public bool operator !=(LSLInteger i1, LSLInteger i2)
            {
                bool ret = i1.value != i2.value;
                return ret;
            }

            static public LSLInteger operator &(LSLInteger i1, LSLInteger i2)
            {
                int ret = i1.value & i2.value;
                return ret;
            }

            public static LSLInteger operator ++(LSLInteger i)
            {
                i.value++;
                return i;
            }


            public static LSLInteger operator --(LSLInteger i)
            {
                i.value--;
                return i;
            }

            static public implicit operator System.Double(LSLInteger i)
            {
                return (double)i.value;
            }

            public static bool operator true(LSLInteger i)
            {
                return i.value != 0;
            }

            public static bool operator false(LSLInteger i)
            {
                return i.value == 0;
            }

            #endregion

            #region Overriders

            public override string ToString()
            {
                return this.value.ToString();
            }

            #endregion
        }

        [Serializable]
        public struct LSLFloat
        {
            public double value;

            #region Constructors

            public LSLFloat(int i)
            {
                this.value = (double)i;
            }

            public LSLFloat(double d)
            {
                this.value = d;
            }

            #endregion

            #region Operators

            static public implicit operator int(LSLFloat f)
            {
                return (int)f.value;
            }

            static public implicit operator uint(LSLFloat f)
            {
                return (uint) Math.Abs(f.value);
            }

            static public implicit operator Boolean(LSLFloat f)
            {
                if (f.value == 0.0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            static public implicit operator LSLFloat(int i)
            {
                return new LSLFloat(i);
            }

            static public implicit operator LSLFloat(string s)
            {
                return new LSLFloat(double.Parse(s));
            }

            static public implicit operator LSLFloat(double d)
            {
                return new LSLFloat(d);
            }

            static public bool operator ==(LSLFloat f1, LSLFloat f2)
            {
                return f1.value == f2.value;
            }

            static public bool operator !=(LSLFloat f1, LSLFloat f2)
            {
                return f1.value != f2.value;
            }

            static public LSLFloat operator ++(LSLFloat f)
            {
                f.value++;
                return f;
            }

            static public LSLFloat operator --(LSLFloat f)
            {
                f.value--;
                return f;
            }

            static public implicit operator System.Double(LSLFloat f)
            {
                return f.value;
            }

            //static public implicit operator System.Int32(LSLFloat f)
            //{
            //    return (int)f.value;
            //}

            #endregion

            #region Overriders

            public override string ToString()
            {
                return String.Format("{0:0.000000}", this.value);
            }

            #endregion
        }
    }
}