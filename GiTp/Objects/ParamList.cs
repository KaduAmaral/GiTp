using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiTp.Objects
{
    class ParamList
    {
        private List<Param> lista = new List<Param>();
        public Int32 Count = 0;

        public ParamList Add(Param item)
        {
            this.lista.Add(item);
            this.Count++;
            return this;
        }

        public ParamList Add(String item)
        {
            return this.Add(new Param(item));
        }

        public Boolean Has(Param item)
        {
            return this.Has(item.Value);
        }

        public Boolean Has(String item)
        {
            bool contem = false;

            foreach (Param p in lista)
            {
                if (p.Value == item)
                {
                    contem = true;
                    break;
                }
            }

            return contem;
            // this.Has(new Param(item));
        }

        public ParamList Remove(Param item)
        {
            this.lista.Remove(item);
            this.Count--;
            return this;
        }

        public ParamList Remove(String item)
        {
            return this.Remove(new Param(item));
        }

        public ParamList Clear()
        {
            this.lista.Clear();
            this.Count = 0;
            return this;
        }

        public Param First()
        {
            return this.lista.First<Param>();
        }

        public String Join(Char glue = ' ')
        {
            string ret = "";
            foreach (Param p in lista)
            {
                ret += p.Value + glue;
            }

            return ret.TrimEnd(glue); ;
        }

        public Param i(int index)
        {
            return this.lista[index];
        }

        public List<Param> List()
        {
            return this.lista;
        }
    }
}
