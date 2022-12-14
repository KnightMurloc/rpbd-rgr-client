using System;
using System.Collections;
using System.Collections.Generic;
using lab2.Models;

namespace lab2
{
    public class EntityIterator<T> where T : IEntity
    {
        private int FirtsId = 0;
        private int LasdId;
        private int PackageSize;
        // private const string HqlTemplateNext = "from {0} where ({1}) and Id > {2} order by Id limit {3}";
        private const string HqlTemplateNext = "from {0} e where {1} e.Id > {2} order by e.Id";
        private const string HqlTemplatePerv = "from {0} e where {1} e.Id < {2} order by e.Id DESC";
        public string Condition { get; set; }

        public EntityIterator(int packageSize)
        {
            PackageSize = packageSize;
            LasdId = PackageSize;
            Condition = "";
        }

        public IList<T> NextPackage()
        {
            
            // string hql = String.Format(
            //     HqlTemplateNext,
            //     typeof(T).Name,
            //     string.IsNullOrEmpty(Condition) ? "" : "(" + Condition + ") and ",
            //     FirtsId
            // );
            
            // Console.WriteLine(hql);
            Query query = new Query {Table = typeof(T).Name, StartId = FirtsId, Count = PackageSize};
            if (Condition != null)
            {
                query.Condition = Condition;
            }

            var result = Repository.Instance.FindByCondition<T>(query);
            if (result.Count != 0)
            {
                FirtsId = result[^1].Id;
                LasdId = result[0].Id;
            }
            return result;
        }
        
        public IList<T> PrevPackage()
        {
            string hql = String.Format(
                HqlTemplatePerv,
                typeof(T).Name,
                string.IsNullOrEmpty(Condition) ? "" : "(" + Condition + ") and ",
                LasdId
            );
            
            Query query = new Query {Table = typeof(T).Name, EndId = LasdId, Count = PackageSize};
            if (Condition != null)
            {
                query.Condition = Condition;
            }
            
            // Console.WriteLine(hql);
            var result = Repository.Instance.FindByCondition<T>(query);
            if (result.Count != 0)
            {
                LasdId = result[^1].Id;
                FirtsId = result[0].Id;
            }
            return result;
        }

        public void Reset()
        {
            FirtsId = 0;
            LasdId = 0;
        }
    }
}