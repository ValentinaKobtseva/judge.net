﻿using System;
using System.Collections.Generic;
using System.Linq;
using Judge.Model.CheckSolution;
using Judge.Model.Contests;
using Judge.Model.Entities;
using Judge.Model.SubmitSolution;
using SubmitResult = Judge.Model.SubmitSolution.SubmitResult;
using Client = Judge.Web.Client;
using SubmitStatus = Judge.Model.SubmitSolution.SubmitStatus;

namespace Judge.Services.Converters;

internal static class SubmitsConverter
{
    public static T Convert<T>(
        SubmitResult submitResult,
        Language language,
        Task task,
        User user,
        IReadOnlyCollection<ContestTask> contestTasks)
        where T : Client.Submits.SubmitResultInfo, new()
    {
        var totalBytes = submitResult.TotalBytes != null
            ? Math.Min(submitResult.TotalBytes.Value, task.MemoryLimitBytes)
            : (int?)null;
        var totalMilliseconds = submitResult.TotalMilliseconds != null
            ? Math.Min(submitResult.TotalMilliseconds.Value, task.TimeLimitMilliseconds)
            : (int?)null;

        var submitResultInfo = new T
        {
            SubmitResultId = submitResult.Id,
            Language = language.Name,
            SubmitDate = submitResult.Submit.SubmitDateUtc,
            ProblemName = task.Name,
            ProblemId = task.Id,
            Status = Convert(submitResult.Status),
            UserId = user.Id,
            UserName = user.UserName
        };

        if (submitResult.Status != SubmitStatus.CompilationError && submitResult.Status != SubmitStatus.ServerError)
        {
            submitResultInfo.TotalBytes = totalBytes;
            submitResultInfo.TotalMilliseconds = totalMilliseconds;
            if (submitResult.Status != SubmitStatus.Accepted)
            {
                submitResultInfo.PassedTests = submitResult.PassedTests;
            }
        }

        if (submitResult.Submit is ContestTaskSubmit contestTaskSubmit)
        {
            submitResultInfo.ContestInfo = new Client.Submits.SubmitResultContestInfo
            {
                ContestId = contestTaskSubmit.ContestId,
                Label = contestTasks.FirstOrDefault(o =>
                                o.ContestId == contestTaskSubmit.ContestId && o.TaskId == contestTaskSubmit.ProblemId)
                            ?.TaskName ??
                        "<unknown>"
            };
        }

        return submitResultInfo;
    }

    private static Client.Submits.SubmitStatus Convert(SubmitStatus status)
    {
        return status switch
        {
            SubmitStatus.Pending => Client.Submits.SubmitStatus.Pending,
            SubmitStatus.CompilationError => Client.Submits.SubmitStatus.CompilationError,
            SubmitStatus.RuntimeError => Client.Submits.SubmitStatus.RuntimeError,
            SubmitStatus.TimeLimitExceeded => Client.Submits.SubmitStatus.TimeLimitExceeded,
            SubmitStatus.MemoryLimitExceeded => Client.Submits.SubmitStatus.MemoryLimitExceeded,
            SubmitStatus.WrongAnswer => Client.Submits.SubmitStatus.WrongAnswer,
            SubmitStatus.Accepted => Client.Submits.SubmitStatus.Accepted,
            SubmitStatus.ServerError => Client.Submits.SubmitStatus.ServerError,
            SubmitStatus.TooEarly => Client.Submits.SubmitStatus.TooEarly,
            SubmitStatus.Unpolite => Client.Submits.SubmitStatus.Unpolite,
            SubmitStatus.TooManyLines => Client.Submits.SubmitStatus.TooManyLines,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}