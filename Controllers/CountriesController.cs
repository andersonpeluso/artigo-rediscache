using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using artigo_rediscache.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace artigo_rediscache.Controllers
{
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private const string CountriesKey = "Countries";

        public CountriesController(IDistributedCache distributedCache)
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

                        var countries = JsonConvert.DeserializeObject<List<Country>>(responseData);

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
                var cliente2 = new ClienteDTO { CPF = "12312312388", Endereco = "Pelotas/RS", Nome = "Rodrigo" };

                using (var redisCliente = new RedisClient("localhost:6379"))
                {
                    redisCliente.Set(cliente1.CPF, cliente1);

                    // Tempo para apagar dados do cliente.
                    //redisCliente.Set(cliente1.CPF, cliente1, new TimeSpan(0, 0, 10));

                    redisCliente.Set(cliente2.CPF, cliente2);

                    var resultadoBusca = redisCliente.Get<ClienteDTO>(cliente1.CPF);

                    return Ok();
                }
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}