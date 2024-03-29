﻿using AutoMapper;
using Data.Enums;
using Data.Models;
using Funtest.Services.Interfaces;
using Funtest.TransferObject.Error.Requests;
using Funtest.TransferObject.Error.Response;
using Funtest.TransferObject.Error.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Funtest.Services
{
    public class ErrorService : Service, IErrorService
    {
        private static string ERROR_PREFIX = "B";
        public readonly IMapper _mapper;

        public ErrorService(IServiceProvider serviceProvider, IMapper mapper) : base(serviceProvider)
        {
            _mapper = mapper;
        }

        public async Task<GetErrorResponse> GetErrorById(Guid id)
        {
            var error = await Context.Errors.FindAsync(id);
            return _mapper.Map<GetErrorResponse>(error);
        }

        public List<GetErrorResponse> GetAllErrors(Guid productId)
        {
            var errors = Context.Errors
                .Where(x => x.Step.TestProcedure.TestCase.ProductId == productId)
                .AsQueryable();

            return errors.Select(x => _mapper.Map<GetErrorResponse>(x)).ToList();
        }

        public List<GetErrorResponse> GetAllErrorsToRetest(Guid productId)
        {
            return Context.Errors.AsQueryable()
                .Where(x => x.Step.TestProcedure.TestCase.ProductId == productId && (x.ErrorState == ErrorState.Retest || x.ErrorState == ErrorState.Fixed))
                .Select(x => _mapper.Map<GetErrorResponse>(x))
                .ToList();
        }

        public List<GetErrorResponse> GetAllErrorsToFix(Guid productId)
        {
            return Context.Errors.AsQueryable()
                .Where(x => x.Step.TestProcedure.TestCase.ProductId == productId && (x.ErrorState == ErrorState.New || x.ErrorState == ErrorState.Reopened))
                .Select(x => _mapper.Map<GetErrorResponse>(x))
                .ToList();
        }

        public List<GetErrorResponse> GetAllErrorsAssignedToDeveloper(string developerId)
        {
            return Context.Errors.AsQueryable()
                .Where(x => x.DeveloperId == developerId && (x.ErrorState == ErrorState.Open || x.ErrorState == ErrorState.Reopened))
                .Select(x => _mapper.Map<GetErrorResponse>(x))
                .ToList();
        }

        public async Task<bool> EditError(Guid id, EditErrorRequest request)
        {
            var error = await Context.Errors.FindAsync(id);

            if (request.Deadline != DateTime.MinValue && request.Deadline > DateTime.Today)
                error.Deadline = request.Deadline;
            else if (request.Deadline == DateTime.Today)
                error.Deadline = DateTime.Today;

            if (request.Description != null)
                error.Description = request.Description;

            if (request.Name != null)
                error.Name = request.Name;

            try
            {
                if (request.ErrorImpact != null)
                    error.ErrorImpact = (ErrorImpact)Enum.Parse(typeof(ErrorImpact), request.ErrorImpact);

                if (request.ErrorPriority != null)
                    error.ErrorPriority = (ErrorPriority)Enum.Parse(typeof(ErrorPriority), request.ErrorPriority);

                if (request.ErrorType != null)
                    error.ErrorType = (ErrorType)Enum.Parse(typeof(ErrorType), request.ErrorType);
            }
            catch
            {
                return false;
            }

            Context.Errors.Update(error);
            if (await Context.SaveChangesAsync() == 0)
                return false;

            return true;
        }

        public async Task<bool> AssignBugToDeveloper(Guid errorId, DeveloperAssignedToErrorRequest request)
        {
            var error = await Context.Errors.FindAsync(errorId);

            if (error == null)
                return false;

            if (error.DeveloperId == null)
            {
                error.DeveloperId = request.DeveloperId;
                error.ErrorState = ErrorState.Open;

                Context.Errors.Update(error);
                if (await Context.SaveChangesAsync() == 0)
                    return false;
                return true;
            }

            return false;
        }

        public async Task<bool> ResolveError(Guid id, ResolveErrorRequest request)
        {
            var error = await Context.Errors.FindAsync(id);
            error.ErrorState = ErrorState.Fixed;
            error.RetestsRequired = request.RetestsRequired;

            Context.Errors.Update(error);
            if (await Context.SaveChangesAsync() == 0)
                return false;

            return true;
        }

        public bool IsErrorExist(Guid id)
        {
            return Context.Errors.Any(x => x.Id == id);
        }

        private List<string> GetDisplayNames(Type type)
        {
            var displaynames = new List<string>();
            var names = Enum.GetNames(type);
            foreach (var name in names)
            {
                var field = type.GetField(name);
                var customAttributes = field.GetCustomAttributes(typeof(DisplayAttribute), true);

                if (customAttributes.Length == 0)
                {
                    displaynames.Add(name);
                }

                foreach (DisplayAttribute attribute in customAttributes)
                {
                    displaynames.Add(attribute.Name);
                }
            }
            return displaynames;
        }

        public List<string> ErrorStates()
        {
            return GetDisplayNames(ErrorState.New.GetType());
        }

        public List<string> ErrorImpacts()
        {
            return Enum.GetValues(typeof(ErrorImpact)).Cast<ErrorImpact>().Select(x => x.ToString()).ToList();
        }

        public List<string> ErrorPriorities()
        {
            return Enum.GetValues(typeof(ErrorPriority)).Cast<ErrorPriority>().Select(x => x.ToString()).ToList();
        }

        public List<string> ErrorTypes()
        {
            return GetDisplayNames(ErrorType.Functional.GetType());
        }

        public async Task<bool> IsErrorNotAssigned(Guid errorId)
        {
            return (await Context.Errors.FindAsync(errorId)).DeveloperId == null;
        }

        public async Task<bool> RejectError(Guid id, DeveloperAssignedToErrorRequest request)
        {
            var error = await Context.Errors.FindAsync(id);

            if (error == null)
                return false;

            if (error.DeveloperId == request.DeveloperId)
            {
                error.ErrorState = ErrorState.Rejected;
                error.DeveloperId = null;

                Context.Errors.Update(error);
                if (await Context.SaveChangesAsync() == 0)
                    return false;
                return true;
            }

            return false;
        }

        public async Task<bool> ResignError(Guid id, DeveloperAssignedToErrorRequest request)
        {
            var error = await Context.Errors.FindAsync(id);

            if (error == null)
                return false;

            if (error.DeveloperId == request.DeveloperId)
            {
                error.DeveloperId = null;
                error.ErrorState = ErrorState.New;

                Context.Errors.Update(error);
                if (await Context.SaveChangesAsync() == 0)
                    return false;
                return true;
            }

            return false;
        }

        private string GetCode(Guid index)
        {
            return $"{ERROR_PREFIX}-{index.ToString().Substring(0, 8).ToUpper()}";
        }

        public async Task<Guid?> AddError(AddErrorRequest request, string testSuiteCategory)
        {
            var error = _mapper.Map<Error>(request);
            var index = Guid.NewGuid();
            error.Id = index;
            error.ErrorState = ErrorState.New;
            error.Code = GetCode(index);
            error.Functionality = testSuiteCategory;
            Context.Errors.Add(error);

            if (await Context.SaveChangesAsync() == 0)
                return null;
            return index;
        }

        public async Task<bool> SetErrorCategory(Error error, string category)
        {
            error.Functionality = category;

            Context.Errors.Update(error);
            if (await Context.SaveChangesAsync() == 0)
                return false;
            return true;
        }

        public async Task<GetErrorTestWithProcedureAndCaseResponse> GetErrorTest(Guid errorId)
        {
            var error = await Context.Errors
                .Include(x => x.Test)
                .Include(x => x.Step)
                .Include(x => x.Step.TestProcedure)
                .Include(x => x.Step.TestProcedure.TestCase)
                .Where(x => x.Id == errorId)
                .FirstAsync();

            GetErrorTestWithProcedureAndCaseResponse errorTest = new GetErrorTestWithProcedureAndCaseResponse();

            errorTest.TestId = (Guid)error.TestId;
            errorTest.TestName = error.Test.Name;
            errorTest.CreationDate = error.Test.CreationDate;
            errorTest.TestCaseId = (Guid)error.Test.TestCaseId;
            errorTest.TestCaseCode = error.Step.TestProcedure.TestCase.Code;


            if (error.Step.TestProcedure.TestCase.EntryDataObject != null)
                errorTest.TestCaseEntryData = error.Step.TestProcedure.TestCase.EntryDataObject.GetValue("data");
            else
                errorTest.TestCaseEntryData = "";

            errorTest.TestCaseProconditions = error.Step.TestProcedure.TestCase.Preconditions;
            errorTest.TestProcedureId = (Guid)error.Step.TestProcedureId;
            errorTest.TestProcedureCode = error.Step.TestProcedure.Code;
            errorTest.Result = error.Step.TestProcedure.Result;
            return errorTest;
        }

        public async Task<Error> GetModelErrorById(Guid id)
        {
            return await Context.Errors.FindAsync(id);
        }

        public async Task<bool> ChangeErrorStatus(Guid id, ErrorState errorState)
        {
            var error = await Context.Errors.FindAsync(id);
            error.ErrorState = errorState;

            Context.Errors.Update(error);
            if (await Context.SaveChangesAsync() == 0)
                return false;
            return true;
        }

        public List<GetIdentityErrorInformationRespons> GetAllErrorsForStep(Guid stepId)
        {
            var errors = Context.Errors
                .Where(x => x.StepId == stepId && (x.ErrorState == ErrorState.Fixed || x.ErrorState == ErrorState.Retest))
                .ToList();
            return errors.Select(x => _mapper.Map<GetIdentityErrorInformationRespons>(x)).ToList();
        }

        public async Task<bool> IsErrorReviewed(Guid errorId, string userId)
        {
            return Context.Reviews.Where(x => x.ErrorId == errorId && x.TesterId == userId && x.IsActual).Count() > 0;
        }

        public async Task<bool> ResignFromEverErrors(string developerId)
        {
            var errorsAssignedToDeveloper = Context.Errors
                .Where(x => x.DeveloperId == developerId && (x.ErrorState == ErrorState.Open || x.ErrorState == ErrorState.Reopened))
                .ToList();

            foreach(var error in errorsAssignedToDeveloper)
            {
                error.ErrorState = ErrorState.New;
                error.DeveloperId = null;
                Context.Errors.Update(error);

                if (await Context.SaveChangesAsync() == 0)
                    return false;
            }
            return true;
        }
    }
}

