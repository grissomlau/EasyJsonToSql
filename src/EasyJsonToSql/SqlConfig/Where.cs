using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace EasyJsonToSql
{
    public class Where
    {
        public Where() {
            this.Fields = new List<Field>();
        }
        [XmlAttribute]
        public bool DeleteChildren { get; set; }
        public List<Field> Fields { get; set; }
    }
}
