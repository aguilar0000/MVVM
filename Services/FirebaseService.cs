using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using MVVM.Models;
using Firebase.Storage;


namespace MVVM.Services
{
    public class FirebaseService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly FirebaseStorage _firebaseStorage;


        public FirebaseService()
        {
            _firebaseClient = new FirebaseClient("https://mvvm-a4d54-default-rtdb.firebaseio.com/products");
            _firebaseStorage = new FirebaseStorage("gs://mvvm-a4d54.appspot.com");
        }



        //Metodo para subir la foto Firebase Storage
        public async Task<string> UploadPhotoAsync(Stream photoStream, string fileName)
        {
            try
            {
                var storageRef = _firebaseStorage.Child("photos").Child(fileName);
                var uploadTask = await storageRef.PutAsync(photoStream);
                var photoUrl = await storageRef.GetDownloadUrlAsync();
               
                return photoUrl.ToString();
            }
            catch (Exception ex)
            {
                // Manejo de errores
                throw new Exception("Error al subir la foto a Firebase Storage.", ex);
            }
        }

       

        //Metodo para obtener productos si es necesario
        public async Task<List<Product>> 
            GetProductsAsync()
        {
            return (await _firebaseClient.Child("products")
                       .OnceAsync<Product>()).Select(item => new Product
                       {
                           Id = item.Key,
                           Nombre = item.Object.Nombre,
                           Descripcion = item.Object.Descripcion,
                           Precio = item.Object.Precio,
                           Foto = item.Object.Foto
                       }).ToList();
        }

        public async Task AddProductAsync(Product product)
        {
            try
            {

           
            await _firebaseClient.Child("products")
                .PostAsync(product);
                //Nombre de Realtime Database
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar el producto  a Firebase Realtime Database", ex);
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
           
                await _firebaseClient
                     .Child("products")
                     .Child(product.Id)//Id producto
                     .PutAsync(product);
                Console.WriteLine("Producto actualizado exitosamente."); 
            
            var productToUpdate = (await _firebaseClient.Child("products").OnceAsync<Product>()).FirstOrDefault(p => p.Object.Id == product.Id);

            if(productToUpdate != null)
            {
                await _firebaseClient.Child("products").Child(productToUpdate.Key)
                .PutAsync(product);
            }
        }

        public async Task DeleteProductAsync(string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId))
                {
                    throw new ArgumentException("El ID del producto no puede ser vacio."); 
                }
                await _firebaseClient
                    .Child("products")
                    .Child(productId)
                    .DeleteAsync();

                Console.WriteLine("Producto eliminado exitosamente."); 
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error al eliminar el producto: {ex.Message}");
                throw; 
            }

            var productToDelete = (await _firebaseClient
                .Child("products")
                .OnceAsync<Product>()).FirstOrDefault(p => p.Object.Id == productId);

            if (productToDelete != null)
            {
                await _firebaseClient
                    .Child("products")
                    .Child(productToDelete.Key)
                    .DeleteAsync();

                Console.WriteLine("Producto eliminado exitosamente.");
            }
        }
    }
}

