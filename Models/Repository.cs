using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class Repository : IRepositoryBase
    {
        // private readonly ISession Session;
        private readonly HttpClient Client = new HttpClient();
        
        private static Repository _instance = null; 
        
        private Repository()
        {
            // Session = NHibernateHelper.OpenSession();
            // Client.BaseAddress = new Uri("http://localhost:5000");
        }
        
        public static Repository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Repository();
                }
        
                return _instance;
            }
        }
        
        public IList<T> FindByCondition<T>(Query query) where T : IEntity
        {

            var body = JsonSerializer.Serialize(query);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:5000/api/list"),
                Content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
            };


            var result = Client.Send(request);

            string json = result.Content.ReadAsStringAsync().Result;
            
            List<T> list = JsonSerializer.Deserialize<List<T>>(json);

            return list;
        }

        public void Create<T>(T entity) where T : IEntity
        {
            entity.Id = -1;
            Update(entity);
        }

        public void Update<T>(T entity) where T : IEntity
        {
            string body = JsonSerializer.Serialize(entity);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://localhost:5000/api/upsert?table={typeof(T).Name}"),
                Content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
            };

            var response = Client.Send(request);
            entity.Id = Convert.ToInt32(response.Content.ReadAsStringAsync().Result);
            // entity = JsonSerializer.Deserialize<T>(response.Content.ReadAsStringAsync().Result);
        }

        public void Delete<T>(T entity) where T : IEntity
        {
            _ = Client.PostAsync($"http://localhost:5000/api/delete?table={typeof(T).Name}&id={entity.Id}",null).Result;
        }

        public T Find<T>(int id) where T : IEntity
        {
            var json = Client.GetAsync($"http://localhost:5000/api/get?table={typeof(T).Name}&id={id}").Result;
            return JsonSerializer.Deserialize<T>(json.Content.ReadAsStringAsync().Result);
        }

        public void Refresh<T>(ref T entity) where T : IEntity
        {
            entity = Find<T>(entity.Id);
        }
    }
}