using MVVM.Services;
using MVVM.ViewModels;
using System;
using MVVM.Models;
using System.Threading.Tasks;
using Firebase.Storage;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace MVVM.Views;

public partial class ProductPage : ContentPage
{
    private readonly FirebaseService _firebaseService;
    private string _selectPhotoUrl;
    private ProductViewModel _viewModels;

    public ProductPage()
    {
        InitializeComponent();
        _viewModels = new ProductViewModel();
        BindingContext = new MVVM.ViewModels.ProductViewModel();
        BindingContext = _viewModels;
        LoadProductsAsync();
        _firebaseService = new FirebaseService();
    }


    private async Task<Stream> PickPhotoAsync()
    {
        var photoPicker = await MediaPicker.PickPhotoAsync();
        return photoPicker != null ? await photoPicker.OpenReadAsync() : null;
    }

    private async void OnSelectPhotoClicked(object sender, EventArgs e)
    {
        var photoStream = await PickPhotoAsync();

        if (photoStream != null)
        {
            var photoName = Guid.NewGuid().ToString() + ".jpg";
            _selectPhotoUrl = await _firebaseService.UploadPhotoAsync(photoStream, photoName);
            photoUrlEntry.Text = _selectPhotoUrl;

          
        }
    }

    private async void OnUpdateProductClicked(object sender, EventArgs e)
    {
        if (_viewModels.SelectedProduct == null)
        {
            await DisplayAlert("Error", "Por favor, seleccione un producto para actualizar.", "OK");
            return;
        }

        // Actualizar el producto con los datos de los campos de entrada
        _viewModels.SelectedProduct.Nombre = NameEntry.Text;
        _viewModels.SelectedProduct.Descripcion = DescriptionEntry.Text;
        _viewModels.SelectedProduct.Precio = decimal.Parse(PriceEntry.Text);
        photoUrlEntry.Text = _viewModels.SelectedProduct.Foto;

        try
        {
            await _viewModels.UpdateProductAsync(_viewModels.SelectedProduct);
            await DisplayAlert("Éxito", "Producto actualizado correctamente", "OK");
            ClearFields(); // Limpiar los campos después de actualizar
            await LoadProductsAsync(); // Cargar productos después de actualizar
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo actualizar el producto: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteProductClicked(object sender, EventArgs e)
    {
        if (_viewModels.SelectedProduct == null)
        {
            await DisplayAlert("Error", "Por favor, seleccione un producto para eliminar.", "OK");
            return;
        }

        var productId = _viewModels.SelectedProduct.Id; // ID del producto a eliminar

        try
        {
            await _viewModels.DeleteProductAsync(productId);
            await DisplayAlert("Éxito", "Producto eliminado correctamente", "OK");
            ClearFields(); // Limpiar los campos después de eliminar
            await LoadProductsAsync(); // Cargar productos después de eliminar
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo eliminar el producto: {ex.Message}", "OK");
        }
    }

   

    private void OnProductSelected(object sender, SelectedItemChangedEventArgs e)
    {
        // Establece el producto seleccionado en el ViewModel
        _viewModels.SelectedProduct = e.SelectedItem as Product;

        if (_viewModels.SelectedProduct != null)
        {
            // Rellena los campos con la información del producto seleccionado
            NameEntry.Text = _viewModels.SelectedProduct.Nombre;
            DescriptionEntry.Text = _viewModels.SelectedProduct.Descripcion;
            PriceEntry.Text = _viewModels.SelectedProduct.Precio.ToString("F2");
            photoUrlEntry.Text = _viewModels.SelectedProduct.Foto;
        }
    }

private void ClearFields()
    {
        NameEntry.Text = string.Empty;
        DescriptionEntry.Text = string.Empty;
        PriceEntry.Text = string.Empty;
        photoUrlEntry.Text = string.Empty;

    }

    private async void OnLoadProductsClicked(object sender, EventArgs e)
    {
       
            await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var products = await _firebaseService.GetProductsAsync();
            Device.BeginInvokeOnMainThread(() =>
            {
                productsListView.ItemsSource = products;
            });


        }
        catch
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los productos:", "Ok");
        }
    }

}