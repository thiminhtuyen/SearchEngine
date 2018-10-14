using Lucene.Net.Search;
using FieldInvertState = Lucene.Net.Index.FieldInvertState;
using System;

namespace CustomizedSeachEngine
{
    public class NewSimilarity : DefaultSimilarity
    {

        /// <summary>Implemented as <c>sqrt(freq)</c>. </summary>
        public override float LengthNorm(string field, int numTerms)
        {
            //return (float)System.Math.Sqrt(freq);

            if (field.Equals("Title"))
                return (float)(0.1 * Math.Log(numTerms));
            else
                return (float)(1 / Math.Sqrt(numTerms));

        }

    }

}