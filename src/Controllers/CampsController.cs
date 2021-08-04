using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    //Nomme la route avec le mot devant "Controller"
    [Route("api/[controller]")]
    
    //Permet le "model binding" avec les données entrantes
    //Permet de faire la validation du modèle selon les attributs ajoutés
    [ApiController]
    public class CampsController : ControllerBase  //Classe pour les controller API
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;

        //Permet de générer des liens vers des actions du controlleur
        private readonly LinkGenerator linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        //public async Task<IActionResult> Get()
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)  //Permet de retourner directement une réponse "OK()" si le type de l'objet de rtour match la definition
        {
            try
            {
                var result = await campRepository.GetAllCampsAsync(includeTalks);

                return mapper.Map<CampModel[]>(result);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("{moniker}")]  //String par défaut, sinon {moniker:int}
        public async Task<ActionResult<CampModel>> Get(string moniker)  //Permet de retourner directement une réponse "OK()" si le type de l'objet de rtour match la definition
        {
            try
            {
                var result = await campRepository.GetCampAsync(moniker);

                return mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> searchByDate(DateTime theDate, bool includeTalks = false)  //Permet de retourner directement une réponse "OK()" si le type de l'objet de rtour match la definition
        {
            try
            {
                var result = await campRepository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!result.Any()) { return NotFound(); }

                return mapper.Map<CampModel[]>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var existingCamp = await campRepository.GetCampAsync(model.Moniker);
                if (existingCamp != null)
                {
                    return BadRequest("Moniker in use");
                }

                var camp = mapper.Map<Camp>(model);
                var location = linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest();
                }

                campRepository.Add(camp);

                if (await campRepository.SaveChangesAsync())
                {

                    return Created(location, mapper.Map<CampModel>(camp));
                }

            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                var camp = await campRepository.GetCampAsync(moniker);

                if (camp == null)
                {
                    return NotFound();
                }

                mapper.Map(model, camp);

                if (await campRepository.SaveChangesAsync())
                {
                    return mapper.Map<CampModel>(camp);
                }
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
            return BadRequest();
        }


        [HttpDelete("{moniker}")]
        public async Task<ActionResult<CampModel>> Delete(string moniker)
        {
            try
            {
                var camp = await campRepository.GetCampAsync(moniker);

                if (camp == null)
                {
                    return NotFound();
                }

                campRepository.Delete<Camp>(camp);

                if (await campRepository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
            return BadRequest();
        }
    }
}


//microsoft.aspnetcore.mvc.versioning