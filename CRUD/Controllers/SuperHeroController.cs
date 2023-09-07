using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using CRUD.Model;
using Dapper;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<ActionResult<List<SuperHero>>> GetAllSuperHeroes()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            IEnumerable<SuperHero> heroes = await SelectAllHeroes(connection);
            return Ok(heroes);
        }

        [HttpGet]
        public async Task<ActionResult<List<SuperHero>>> GetSuperHero(int heroId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var hero = await connection.QueryFirstAsync<SuperHero>("select * from superheroes where id = @Id", 
                        new{Id = heroId});
            return Ok(hero);
        }

        [HttpPost]
        public async Task<ActionResult<List<SuperHero>>> CreateSuperHeroes(SuperHero superHero)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("insert into superheroes (name, firstname , lastname , place) values (@Name, @FirstName, @LastName, @Place)", superHero);
            return Ok(await SelectAllHeroes(connection));
        }

        [HttpPost]
        public async Task<ActionResult<List<SuperHero>>> UpdateSuperHeroes(SuperHero superHero)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("update superheroes set name = @Name, firstname =@FirstName, lastname= @LastName, place = @Place where id = @Id", superHero);
            return Ok(await SelectAllHeroes(connection));
        }
         private static async Task<IEnumerable<SuperHero>> SelectAllHeroes(SqlConnection connection)
        {
            return await connection.QueryAsync<SuperHero>("select * from superheroes");
        }
    } 
}