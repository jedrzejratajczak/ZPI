﻿using Funtest.TransferObject.TestCase.Requests;
using Funtest.TransferObject.TestCase.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Funtest.Services.Interfaces
{
    public interface ITestCaseService
    {
        Task<bool> AddTestCase(AddTestCaseRequest testCaseRequest);
        Task<bool> ExistTestCase(Guid id);
        Task<GetTestCaseResponse> GetTestCaseById(Guid id);
        Task<bool> EditTestCase(Guid id, EditTestCaseRequest request);
        List<GetTestCaseIdentityValueResponse> GetAllTestCasesForProduct(Guid productId);
        bool IsEditPossible(Guid testCaseId);
        Task<Guid?> CreateNewTestCaseBaseOnExistTCWithModification(Guid testCaseId, EditTestCaseRequest editedTestCase);
    }
}
