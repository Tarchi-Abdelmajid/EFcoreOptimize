﻿using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using OptimizeMePlease.Context;
using System.Collections.Generic;
using System.Linq;

namespace OptimizeMePlease
{
    [MemoryDiagnoser]
    public class BenchmarkService
    {
        public BenchmarkService()
        {
        }

        /// <summary>
        /// Get top 2 Authors (FirstName, LastName, UserName, Email, Age, Country) 
        /// from country Serbia aged 27, with the highest BooksCount
        /// and all his/her books (Book Name/Title and Publishment Year) published before 1900
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public List<AuthorDTO> GetAuthors()
        {
            using var dbContext = new AppDbContext();

            var authors = dbContext.Authors
                                        .Include(x => x.User)
                                        .ThenInclude(x => x.UserRoles)
                                        .ThenInclude(x => x.Role)
                                        .Include(x => x.Books)
                                        .ThenInclude(x => x.Publisher)
                                        .ToList()
                                        .Select(x => new AuthorDTO
                                        {
                                            UserCreated = x.User.Created,
                                            UserEmailConfirmed = x.User.EmailConfirmed,
                                            UserFirstName = x.User.FirstName,
                                            UserLastActivity = x.User.LastActivity,
                                            UserLastName = x.User.LastName,
                                            UserEmail = x.User.Email,
                                            UserName = x.User.UserName,
                                            UserId = x.User.Id,
                                            RoleId = x.User.UserRoles.First(y => y.UserId == x.UserId).RoleId,
                                            BooksCount = x.BooksCount,
                                            AllBooks = x.Books.Select(y => new BookDto
                                            {
                                                Id = y.Id,
                                                Name = y.Name,
                                                Published = y.Published,
                                                ISBN = y.ISBN,
                                                PublisherName = y.Publisher.Name
                                            }).ToList(),
                                            AuthorAge = x.Age,
                                            AuthorCountry = x.Country,
                                            AuthorNickName = x.NickName,
                                            Id = x.Id
                                        })
                                        .ToList()
                                        .Where(x => x.AuthorCountry == "Serbia" && x.AuthorAge == 27)
                                        .ToList();

            var orderedAuthors = authors.OrderByDescending(x => x.BooksCount).ToList().Take(2).ToList();

            var finalAuthors = new List<AuthorDTO>();
            foreach (var author in orderedAuthors)
            {
                var books = new List<BookDto>();

                var allBooks = author.AllBooks;

                foreach (var book in allBooks)
                {
                    if (book.Published.Year >= 1900) continue;
                    book.PublishedYear = book.Published.Year;
                    books.Add(book);
                }

                author.AllBooks = books;
                finalAuthors.Add(author);
            }

            return finalAuthors;
        }

        [Benchmark]
        public List<AuthorDTO> GetAuthors_Optimized()
        {
            using var dbContext = new AppDbContext();
            var authors = new List<AuthorDTO>();
            var authorsDb = dbContext.Authors.AsNoTracking().Include(b => b.Books.Where(x => x.Published.Year < 1900))
                .Where(x => x.Country == "Serbia" && x.Age == 27).OrderByDescending(x => x.BooksCount).Take(2)
                .AsEnumerable();
            foreach (var auth in authorsDb)
            {
                var authorDto = new AuthorDTO
                {
                    Id = auth.Id,
                    UserLastName = auth.User.LastName,
                    UserEmail = auth.User.Email,
                    UserName = auth.User.UserName,
                    UserId = auth.User.Id,
                    AllBooks = auth.Books.Select(y => new BookDto
                    {
                        Id = y.Id,
                        Name = y.Name,
                        Published = y.Published,
                    }).ToList(),
                    AuthorAge = auth.Age,
                    AuthorCountry = auth.Country,
                };
                authors.Add(authorDto);
            }
                
            return authors;
        }
    }
}
