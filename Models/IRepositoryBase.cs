using System.Collections.Generic;

namespace lab2.Models
{
    public interface IRepositoryBase
    {
        IList<T> FindByCondition<T>(Query query) where T : IEntity;
        void Create<T>(T entity) where T : IEntity;
        void Update<T>(T entity) where T : IEntity;
        void Delete<T>(T entity) where T : IEntity;
        public T Find<T>(int id) where T : IEntity;
        public void Refresh<T>(ref T entity) where T : IEntity;
    }
}