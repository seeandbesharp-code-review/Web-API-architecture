using AutoMapper;
using DTO_s;
using Entities;
using Repositories;
using StackExchange.Redis;
using System.Text.Json;

namespace Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IDatabase _redisDb;
        private static readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);

        public ProductService(IProductRepository productRepository, IMapper mapper, IConnectionMultiplexer redis)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _redisDb = redis.GetDatabase();
        }

        public async Task<ProductDTO> GetProductById(int id)
        {
            string key = $"product:{id}";

            try
            {
                // Cache Hit
                RedisValue cached = await _redisDb.StringGetAsync(key);
                if (cached.HasValue)
                {
                    return JsonSerializer.Deserialize<ProductDTO>(cached!)!;
                }
            }
            catch (RedisException) { }

            // Cache Miss – fetch from DB
            Product p = await _productRepository.GetProductById(id);
            ProductDTO result = _mapper.Map<Product, ProductDTO>(p);

            try
            {
                await _redisDb.StringSetAsync(key, JsonSerializer.Serialize(result), _ttl);
            }
            catch (RedisException) { }

            return result;
        }

        public async Task<PageResponse<ProductDTO>> GetProducts(string? name, int[]? categories, int? minPrice, int? maxPrice, int? position, int? skip, string? orderBy, string? description)
        {
            skip = skip ?? 10;
            position = position ?? 1;

            string categoriesKey = categories != null ? string.Join(",", categories) : "null";
            string key = $"products:{name}:{categoriesKey}:{minPrice}:{maxPrice}:{position}:{skip}:{orderBy}:{description}";

            try
            {
                // Cache Hit
                RedisValue cached = await _redisDb.StringGetAsync(key);
                if (cached.HasValue)
                {
                    return JsonSerializer.Deserialize<PageResponse<ProductDTO>>(cached!)!;
                }
            }
            catch (RedisException) { }

            // Cache Miss – fetch from DB
            List<Product> products;
            PageResponse<ProductDTO> pageResponse = new PageResponse<ProductDTO>();
            (products, pageResponse.TotalItems) = await _productRepository.GetProducts(name, categories, minPrice, maxPrice, (int)position, (int)skip, orderBy, description);
            pageResponse.Data = _mapper.Map<List<Product>, List<ProductDTO>>(products);
            pageResponse.CurrentPage = (int)position;
            pageResponse.HasPreviousPage = pageResponse.CurrentPage > 1;
            pageResponse.HasNextPage = (pageResponse.TotalItems / skip) > (pageResponse.CurrentPage - 1);
            pageResponse.PageSize = (int)skip;

            try
            {
                await _redisDb.StringSetAsync(key, JsonSerializer.Serialize(pageResponse), _ttl);
            }
            catch (RedisException) { }

            return pageResponse;
        }
    }
}
