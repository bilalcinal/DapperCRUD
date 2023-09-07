using System.Data.SqlClient;
using CRUD.Model;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Z.Dapper.Plus;


namespace CRUD.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SuperHeroController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public SuperHeroController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #region GetAllSuperHeroes
        [HttpGet]
        public async Task<ActionResult<List<SuperHero>>> GetAllSuperHeroes()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            IEnumerable<SuperHero> heroes = await SelectAllHeroes(connection);
            return Ok(heroes);
        }
        #endregion
        #region GetSuperHero QueryFirstAsync 
        //QueryFirstAsync and Select
        [HttpGet]
        public async Task<ActionResult<List<SuperHero>>> GetSuperHero(int heroId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var hero = await connection.QueryFirstAsync<SuperHero>("select * from superheroes where id = @Id", 
                        new{Id = heroId});
            return Ok(hero);
        }
        #endregion
        #region BulkInsertSuperHeroes BulkInsert
        //BulkInsert
        [HttpPost]
        public async Task<ActionResult<List<SuperHero>>> BulkInsertSuperHeroes(List<SuperHero> superHeroes)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            try
            {
                DapperPlusManager.Entity<SuperHero>().Table("superheroes");
                connection.BulkInsert(superHeroes); 

                var allSuperHeroes = await SelectAllHeroes(connection);

                return Ok(allSuperHeroes);
            }
            finally
            {
                connection.Close();
            }
        }
        #endregion
        #region CrateSuperHeroes ExecuteAsync Insert
        //ExecuteAsync and Insert
        [HttpPost]
        public async Task<ActionResult<List<SuperHero>>> CreateSuperHeroes(SuperHero superHero)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("insert into superheroes (name, firstname , lastname , place) values (@Name, @FirstName, @LastName, @Place)", superHero);
            return Ok(await SelectAllHeroes(connection));
        }
        #endregion        
        #region UpdateSuperHeroes ExecuteAsync Update
        //ExecuteAsync and Update
        [HttpPost]
        public async Task<ActionResult<List<SuperHero>>> UpdateSuperHeroes(SuperHero superHero)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("update superheroes set name = @Name, firstname =@FirstName, lastname= @LastName, place = @Place where id = @Id", superHero);
            return Ok(await SelectAllHeroes(connection));
        }
        #endregion
        #region BulkUpdateSuperHeroes BulkUpdate
        //BulkUpdate
        [HttpPost]
        public async Task<ActionResult<List<SuperHero>>> BulkUpdateSuperHeroes(List<SuperHero> superHeroes)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            try
            {
                DapperPlusManager.Entity<SuperHero>().Table("superheroes"); // Tablo adını belirtin
                connection.BulkUpdate(superHeroes); // Dapper.Plus kullanarak toplu güncelleme yapın

                var allSuperHeroes = await SelectAllHeroes(connection);

                return Ok(allSuperHeroes);
            }
            finally
            {
                connection.Close();
            }
        }
        #endregion
        #region DeleteSuperHeroes ExecuteAsync Delete
        //ExecuteAsync and Delete        
        [HttpPost]
        public async Task<ActionResult<List<SuperHero>>> DeleteSuperHeroes(int id)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("DELETE FROM superheroes WHERE id = @Id", new { Id = id });
            return Ok(await SelectAllHeroes(connection));
        }
        #endregion
        #region SelectAllHeroes QueryAsync
        //QueryAsync and Selecet
         private static async Task<IEnumerable<SuperHero>> SelectAllHeroes(SqlConnection connection)
        {
            return await connection.QueryAsync<SuperHero>("select * from superheroes");
        }
        #endregion
        #region TotalSuperHeroesName ExecuteScalarAsync
        //ExecuteScaller and Count 
        [HttpPost]
        public async Task<ActionResult> TotalSuperHeroesName()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM superheroes");
            return Ok(count);
        }
        #endregion
        #region GetSuperHeroPlace QuerySingleAsync
        // QuerySingleAsync and Select
        [HttpGet]
        public async Task<ActionResult<string>> GetSuperHeroPlace(int heroId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var place = await connection.QuerySingleAsync<string>("SELECT Place FROM superheroes WHERE Id = @Id", 
                        new { Id = heroId });
            return Ok(place);
        }
        #endregion
        #region CrateSuperVillians ExecuteAsync
        //ExecuteAsync and Insert
        [HttpPost]
        public async Task<ActionResult<SuperVillain>> CrateSuperVillains(SuperVillain superVillain)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("insert into supervillains (name, firstname , lastname , place) values (@Name, @FirstName, @LastName, @Place)", superVillain);
            return Ok();
        }
        #endregion         
        #region GetAllSupers QueryMultipleAsync
        //QueryMultiple and read
        [HttpGet]
        public async Task<ActionResult> GetAllSupers()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var query = @"  SELECT * FROM superheroes;SELECT * FROM supervillains;";
            
            using var multi = await connection.QueryMultipleAsync(query);
            var superHeroes = multi.Read<SuperHero>().ToList();
            var superVillains = multi.Read<SuperVillain>().ToList();
            var superList = new { SuperHeroes = superHeroes, SuperVillains = superVillains };
            
            return Ok(superList);
        }
        #endregion
        #region GetSuperVillain ExecuteReader
        // ExecuteReader 
        [HttpGet]
        public async Task<ActionResult<List<SuperVillain>>> GetSuperVillain(int villainId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            var sql = "SELECT * FROM supervillains WHERE id = @Id";
            using var reader = await connection.ExecuteReaderAsync(sql, new { Id = villainId });

            var villains = new List<SuperVillain>();
            while (reader.Read())
            {
                var villain = new SuperVillain
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Place = reader.GetString(reader.GetOrdinal("Place")),
                };

                villains.Add(villain);
            }

            return Ok(villains);
        }
        #endregion
       
    } 
}
