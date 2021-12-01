using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Gestor.RedisCache.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace Gestor.RedisCache.Controllers
{
    [Route("api/[controller]")]
    public class CapitalController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private const string CountriesKey = "Countries";

        public CapitalController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                var countriesObject = await _distributedCache.GetStringAsync(CountriesKey);

                if (!string.IsNullOrWhiteSpace(countriesObject))
                {
                    return Ok(countriesObject);
                }
                else
                {
                    const string restCountriesUrl = "https://restcountries.eu/rest/v2/all";

                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.GetAsync(restCountriesUrl);

                        var responseData = await response.Content.ReadAsStringAsync();

                        var countries = JsonConvert.DeserializeObject<List<Regiao>>(responseData);

                        var memoryCacheEntryOptions = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                            SlidingExpiration = TimeSpan.FromSeconds(1200)
                        };

                        await _distributedCache.SetStringAsync(CountriesKey, responseData, memoryCacheEntryOptions);

                        return Ok(countries);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet(nameof(TesteRedis))]
        public async Task<IActionResult> TesteRedis()
        {
            try
            {
                var cliente1 = new ClienteDTO { CPF = "12312312387", Endereco = "São Paulo/SP", Nome = "Anderson" };

                LojaDTO loja1 = new LojaDTO();
                loja1.Id = 01;
                loja1.Descricao = "Loja 01 de teste do cliente 1";

                cliente1.Lojas.Add(loja1);

                var cliente2 = new ClienteDTO { CPF = "12312312388", Endereco = "Pelotas/RS", Nome = "Rodrigo" };

                LojaDTO loja2 = new LojaDTO();
                loja2.Id = 11;
                loja2.Descricao = "Loja 02 de teste do cliente 1";

                cliente2.Lojas.Add(loja2);

                using (var redisCliente = new RedisClient("localhost:6379"))
                {
                    redisCliente.Set(cliente1.CPF, cliente1);

                    // Tempo para apagar dados do cliente.
                    //redisCliente.Set(cliente1.CPF, cliente1, new TimeSpan(0, 0, 10));

                    redisCliente.Set(cliente2.CPF, cliente2, new TimeSpan(0, 0, 15));

                    var resultadoBusca = redisCliente.Get<ClienteDTO>(cliente1.CPF);

                    // return Created("http://example.org/myitem", new { Nome = "teste item" });

                    return Created(string.Empty, string.Empty);
                }
            }
            catch
            {
                throw;
            }
        }

        [HttpGet(nameof(BuscarRedis))]
        public async Task<IActionResult> BuscarRedis(string chave)
        {
            try
            {
                using (var redisCliente = new RedisClient("localhost:6379"))
                {
                    var resultadoBusca = redisCliente.Get<ClienteDTO>(chave);

                    return Ok(resultadoBusca);
                }
            }
            catch
            {
                throw;
            }
        }

    }
}