using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;

namespace MongoDBProject.Test
{
    [TestClass]
    public class MongoDBTests
    {
        #region Variables

        private static readonly string MongoDatabaseName = ConfigurationManager.AppSettings["MongoDatabaseName"].ToString();
        private static readonly string MongoUsername = ConfigurationManager.AppSettings["MongoUsername"].ToString();
        private static readonly string MongoPassword = ConfigurationManager.AppSettings["MongoPassword"].ToString();
        private static readonly int MongoPort = Convert.ToInt32(ConfigurationManager.AppSettings["MongoPort"]);
        private static readonly string MongoHost = ConfigurationManager.AppSettings["MongoHost"].ToString();
        private readonly MongoContext db = new MongoContext(MongoDatabaseName, MongoUsername, MongoPassword, MongoPort, MongoHost);

        #endregion

        #region Pruebas Unitarias

        [TestMethod]
        public void InsertarRegistroEsperaVerdaderoTest()
        {
            var inserted = InsertarRegistro("Josh");
            Assert.IsTrue(inserted.exito);
        }

        [TestMethod]
        public void EditarRegistroEsperaVerdaderoTest()
        {
            string nombre = "Hugo";
            InsertarRegistro(nombre);
            var updated = EditarRegistro(nombre);
            Assert.IsTrue(updated.exito);
        }

        [TestMethod]
        public void ObtenerListaEstudiantesEsperaListaNoVaciaTest()
        {
            string nombre = "Paco";
            InsertarRegistro(nombre);
            var table = db.UseTable("students");
            var lista = db.GetAll<StudentModel>(table);
            Assert.IsNotNull(lista);
            Assert.AreNotSame(0, lista.Count);
        }

        [TestMethod]
        public void ObtenerListaEstudiantesConFiltroEsperaListaNoVaciaTest()
        {
            string nombre = "Pedro";
            InsertarRegistro(nombre);
            var table = db.UseTable("students");
            var builder = Builders<StudentModel>.Filter;
            var filter = builder.Where(e => e.firstname.Equals(nombre));
            var lista = db.GetAllWithFilter(table, filter);
            Assert.IsNotNull(lista);
            Assert.AreNotSame(0, lista.Count);
        }

        [TestMethod]
        public void EliminarRegistroEsperaVerdaderoTest()
        {
            string nombre = "Mayra";
            InsertarRegistro(nombre);
            var table = db.UseTable("students");
            var builder = Builders<StudentModel>.Filter;
            var filter = builder.Where(e => e.firstname.Equals(nombre));
            var lista = db.GetAllWithFilter(table, filter);
            var deleted = db.Delete(table, lista.First().id);
            Assert.IsTrue(deleted.exito);
        }

        [TestMethod]
        public void InsertarYEditarRegistroEsperaVerdaderoTest()
        {
            string nombre = "Laura";
            var table = db.UseTable("students");

            var student = new StudentModel()
            {
                firstname = nombre,
                lastname = "Pitt",
                subjects = new List<string>() { "Physics" },
                klass = "JSS 4",
                age = 28
            };

            var inserted = db.Save(table, student);
            Assert.IsTrue(inserted.exito);

            var builder = Builders<StudentModel>.Filter;
            var filter = builder.Where(e => e.firstname.Equals(nombre));
            var lista = db.GetAllWithFilter(table, filter);

            var studentUpdate = lista.First();
            studentUpdate.firstname = student.firstname + " modificado";
            studentUpdate.subjects.Add("nuevo");
            studentUpdate.subjects.Add("nuevo 2");

            var updated = db.Save(table, studentUpdate);
            Assert.IsTrue(updated.exito);
        }

        [TestMethod]
        public void InsertarListaRegistroEsperaVerdaderoTest()
        {
            var inserted = InsertarListaRegistros("Josh");
            Assert.IsTrue(inserted.exito);
        }

        [TestMethod]
        public void EditarListaRegistroEsperaVerdaderoTest()
        {
            string nombre = "Cynthia";
            InsertarListaRegistros(nombre);
            var table = db.UseTable("students");
            var builder = Builders<StudentModel>.Filter;
            var filter = builder.Where(e => e.firstname.Contains(nombre));
            var studentList = db.GetAllWithFilter(table, filter);

            studentList.ForEach(s =>
            {
                s.lastname = "modificado";
            });

            var updated = db.UpdateMany(table, studentList);
            Assert.IsTrue(updated.exito);
        }

        [TestMethod]
        public void EliminarListaRegistroEsperaVerdaderoTest()
        {
            string nombre = "Rachell";
            InsertarListaRegistros(nombre);
            var table = db.UseTable("students");
            var builder = Builders<StudentModel>.Filter;
            var filter = builder.Where(e => e.firstname.Contains(nombre));
            var studentList = db.GetAllWithFilter(table, filter);

            var deleted = db.DeleteMany(table, MongoExtensions.GetObjectIdList(studentList));
            Assert.IsTrue(deleted.exito);
        }

        [TestMethod]
        public void InsertarYEditarListaRegistroEsperaVerdaderoTest()
        {
            string nombre = "Laura";
            var table = db.UseTable("students");

            var studentList = new List<StudentModel>();

            studentList.Add(new StudentModel()
            {
                firstname = nombre + " 1",
                lastname = "Pitt",
                subjects = new List<string>() { "Physics" },
                klass = "JSS 3",
                age = 28
            });
            studentList.Add(new StudentModel()
            {
                firstname = nombre + " 2",
                lastname = "Pitt",
                subjects = new List<string>() { "Physics" },
                klass = "JSS 3",
                age = 28
            });
            studentList.Add(new StudentModel()
            {
                firstname = nombre + " 3",
                lastname = "Pitt",
                subjects = new List<string>() { "Physics" },
                klass = "JSS 3",
                age = 28
            });

            var inserted = db.SaveMany(table, studentList);
            Assert.IsTrue(inserted.exito);

            var builder = Builders<StudentModel>.Filter;
            var filter = builder.Where(e => e.firstname.Contains(nombre));
            var studentListUpdate = db.GetAllWithFilter(table, filter);

            studentListUpdate.ForEach(s =>
            {
                s.lastname = "modificado";
            });

            var updated = db.SaveMany(table, studentListUpdate);
            Assert.IsTrue(updated.exito);
        }

        #endregion

        #region Metodos Privados

        private MongoResponse InsertarRegistro(string nombre)
        {
            var table = db.UseTable("students");

            var student = new StudentModel()
            {
                firstname = nombre,
                lastname = "Pitt",
                subjects = new List<string>() { "Physics" },
                klass = "JSS 3",
                age = 28
            };

            return db.Insert(table, student);
        }

        private MongoResponse EditarRegistro(string nombre)
        {
            var table = db.UseTable("students");

            var builder = Builders<StudentModel>.Filter;
            var filter = builder.Where(e => e.firstname.Equals(nombre));
            var lista = db.GetAllWithFilter(table, filter);
            var student = lista.First();
            student.firstname = student.firstname + " modificado";
            student.subjects.Add("nuevo");
            return db.Update(table, student);
        }

        private MongoResponse InsertarListaRegistros(string nombre)
        {
            var table = db.UseTable("students");

            var studentList = new List<StudentModel>();

            studentList.Add(new StudentModel()
            {
                firstname = nombre + " 1",
                lastname = "Pitt",
                subjects = new List<string>() { "Physics" },
                klass = "JSS 3",
                age = 28
            });
            studentList.Add(new StudentModel()
            {
                firstname = nombre + " 2",
                lastname = "Pitt",
                subjects = new List<string>() { "Physics" },
                klass = "JSS 3",
                age = 28
            });
            studentList.Add(new StudentModel()
            {
                firstname = nombre + " 3",
                lastname = "Pitt",
                subjects = new List<string>() { "Physics" },
                klass = "JSS 3",
                age = 28
            });

            return db.InsertMany(table, studentList);
        }

        #endregion
    }
}
