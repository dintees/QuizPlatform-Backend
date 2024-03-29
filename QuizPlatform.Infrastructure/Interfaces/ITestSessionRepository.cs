﻿using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface ITestSessionRepository
{
    Task<TestSession?> GetBySessionIdAsync(int testSessionId, bool includeTest = false);
    Task<List<TestSession>?> GetByTestIdAsync(int testId, bool includeTest = false);
    Task<List<TestSession>> GetByUserIdWithTestAsync(int userId, bool sortByModificationTime = false);
    Task AddAsync(TestSession testSession);
    Task<bool> SaveAsync();
}