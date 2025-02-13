﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tusk.Api.Exceptions;
using Tusk.Api.Models;
using Tusk.Api.Persistence;
using Tusk.Api.Stories.Commands;
using Tusk.Api.Stories.Queries;
using Tusk.Api.Tests.Common;
using Xunit;

namespace Tusk.Api.Tests.Controllers
{
    public abstract class UserStoryTests
    {
        public UserStoryTests(DbContextOptions<TuskDbContext> contextOptions)
        {
            ContextOptions = contextOptions;

            Seed();
        }
        protected DbContextOptions<TuskDbContext> ContextOptions { get; }

        private void Seed()
        {
            using var context = new TuskDbContext(ContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var story = new UserStory(
                "My demo user story",
                Priority.Create(1).Value,
                "Info",
                "Provide long text here",
                BusinessValue.BV900);
            var task = new StoryTask("My long description");
            story.AddTask(task);
            context.Stories.Attach(story);
            context.SaveChanges();
        }

        [Fact]
        public async Task Stories_Success_ListOfStories()
        {
            // Arrange
            using var context = new TuskDbContext(ContextOptions);
            var profiles = new List<Profile>{ new UserStoriesProfile() };

            var handler = new GetAllStoriesQueryHandler(
                context,
                TestFactory.GetMapper(profiles),
                TestFactory.GetDtInstance());

            // Act
            var result = await handler.Handle(new GetAllStoriesQuery(), new System.Threading.CancellationToken());

            // Assert
            result.Data.Count().Should().Be(1);
        }

        [Fact]
        public async Task Create_Success_ReturnsStoryId()
        {
            // Arrange
            using var context = new TuskDbContext(ContextOptions);
            var mediator = new Mock<IMediator>();
            var command = new CreateStoryCommand
            {
                Importance = UserStory.Relevance.CouldHave,
                Text = "My demo post user story",
                Title = "Demo post",
                BusinessValue = 1
            };

            var handler = new CreateStoryCommandHandler(context, mediator.Object);

            // Act
            var result = await handler.Handle(command, new System.Threading.CancellationToken());

            // Assert
            result.Should().BeGreaterOrEqualTo(0);
            context.Stories.Count().Should().Be(2);
        }

        [Fact]
        public async Task Create_InvalidImportance_ReturnsValidationError()
        {
            // Arrange
            var validator = new CreateStoryValidator();
            var command = new CreateStoryCommand
            {
                Importance = (UserStory.Relevance)5,
                Text = "My demo post user story",
                Title = "Demo post",
                BusinessValue = 1
            };

            // Act
            var result = await validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Create_InvalidBusinessValue_ReturnsValidationError()
        {
            // Arrange
            var validator = new CreateStoryValidator();
            var command = new CreateStoryCommand
            {
                Importance = UserStory.Relevance.CouldHave,
                Text = "My demo post user story",
                Title = "Demo post",
                BusinessValue = 10
            };

            // Act
            var result = await validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Story_Success_ProjectFound()
        {
            // Arrange
            using var context = new TuskDbContext(ContextOptions);
            var profiles = new List<Profile>{ new UserStoryProfile() };

            var handler = new GetStoryQueryHandler(context, TestFactory.GetMapper(profiles));

            // Act
            var result = await handler.Handle(new GetStoryQuery(1), new System.Threading.CancellationToken());

            result.Story.Should().NotBeNull();
            result.Story.Tasks.Count().Should().BeGreaterThan(0);
            result.Story.Title.Should().Be("My demo user story");
            result.Story.Priority.Should().Be(1);
            result.Story.BusinessValue.Should().Be("Business Value 900");
        }

        [Fact]
        public async Task Story_InvalidId_ProjectNotFound()
        {
            // Arrange
            using var context = new TuskDbContext(ContextOptions);
            var profiles = new List<Profile>{ new UserStoryProfile() };

            var handler = new GetStoryQueryHandler(context, TestFactory.GetMapper(profiles));

            // Act
            Task action() => handler.Handle(new GetStoryQuery(-100), new System.Threading.CancellationToken());

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(action);
        }

    }
}
