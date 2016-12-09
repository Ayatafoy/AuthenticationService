﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using InternshipAuthenticationService.Repository;
using InternshipAuthenticationService.Models.Exceptions;
using InternshipAuthenticationService.Models.EFModels;

namespace InternshipAuthenticationService.DAL
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthenticationServiceDbContext _db;
        
        public UserRepository(AuthenticationServiceDbContext context)
        {
            _db = context;
            if (!_db.Database.Exists())
            {
                _db.Database.Create();
            }
        }

        public IQueryable<User> GetAll()
        {
            return _db.Users.Include(o => o.Roles);
        }

        public IEnumerable<User> Search(String login, String fullName, String role)
        {
            IQueryable<User> users = _db.Users.Include(o => o.Roles);
            if (!string.IsNullOrEmpty(login))
                users = users.Where(s => s.Login == login);
            if (!string.IsNullOrEmpty(fullName))
            {
                users = users.Where(s => s.FullName == fullName);
            }
            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(s => s.Roles.Any(r => r.RoleName.Equals(role)));
            }
            return users.ToList();
        }

        public User GetById(int userId)
        {
            return _db.Users.Where(s => s.Id == userId).FirstOrDefault<User>();
        }

        public User GetByLogin(string login)
        {
            return _db.Users.Where(s => s.Login == login).FirstOrDefault<User>();
        }        

        public void Create(User user)
        {
            UpdateUserRoles(user);
            _db.Users.Add(user);
            _db.SaveChanges();
        }

        public void Update(User user)
        {
            UpdateUserRoles(user);
            User newUser = new User();
            newUser = GetById(user.Id);
            newUser.Login = user.Login;
            newUser.FullName = user.FullName;
            foreach (Role role in newUser.Roles.Where(r => !user.Roles.Any(ur => ur.Id == r.Id)))
            {
                role.Users.Remove(newUser);
            }
            newUser.Roles = user.Roles;
            _db.SaveChanges();
        }

        public void Delete(User user)
        {
            User newUser = _db.Users.Where(s => s.Login == user.Login).FirstOrDefault<User>();
            if (newUser != null)
            {
                _db.Users.Remove(newUser);
                _db.SaveChanges();
            }
        }

        private void UpdateUserRoles(User user)
        {
            string[] roles = user.Roles.Select(r => r.RoleName).Distinct().ToArray();
            List<Role> rolesList = _db.Roles.Where(r => roles.Contains(r.RoleName)).ToList();
            if (roles.Length != rolesList.Count)
                throw new InvalidRoleException("This is role not exists");

            user.Roles = rolesList;
        }
    }
}
