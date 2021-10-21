﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Funtest.Services.Interfaces;
using Funtest.TransferObject.TestProcedure.Requests;
using Funtest.TransferObject.TestProcedure.Responses;
using Funtest.Interfaces;
using System.Linq;

namespace Funtest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestProceduresController : ControllerBase
    {
        private readonly ITestProcedureService _testProcedureService;
        private readonly IStepService _stepService;

        public TestProceduresController(ITestProcedureService testProcedureService, IStepService stepService)
        {
            _testProcedureService = testProcedureService;
            _stepService = stepService;
        }

        [HttpPost]
        public async Task<ActionResult> PostTestProcedure(AddTestProcedureRequest testProcedure)
        {
            var correctResult = await _testProcedureService.AddTestProcedure(testProcedure);

            if (correctResult)
                return Ok();

            return Problem("Problem with saving an object in the database");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetTestProcedureResponse>> GetTestProcedure(Guid id)
        {
            var testProcedure = await _testProcedureService.GetTestProcedureById(id);
            if (testProcedure == null)
            {
                return NotFound("Object with the given id doesn't exist.");
            }
            testProcedure.StepIds = _stepService.GetAllStepsForTestProcedure(id).Select(x => x.Id).ToList();

            return Ok(testProcedure);
        }
    }
}