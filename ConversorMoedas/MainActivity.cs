using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Net;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ConversorMoedas
{
    [Activity(Label = "ConversorMoedas", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private string UrlApi = "https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20csv%20where%20url%3D%22http%3A%2F%2Ffinance.yahoo.com%2Fd%2Fquotes.csv%3Fe%3D.csv%26f%3Dc4l1%26s%3D{0}{1}%3DX%22%3B&format=json&diagnostics=true&callback=";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Spinner moeda1 = FindViewById<Spinner>(Resource.Id.moeda1);
            Spinner moeda2 = FindViewById<Spinner>(Resource.Id.moeda2);

            moeda1.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(moeda1_ItemSelected);
            moeda2.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(moeda2_ItemSelected);

            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.moedas_array, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            moeda1.Adapter = adapter;
            moeda2.Adapter = adapter;


            EditText txtQuantidade = FindViewById<EditText>(Resource.Id.txtQuantidade);

            Button btnConverter = FindViewById<Button>(Resource.Id.btnConverter);
            btnConverter.Click += btnConverter_Click;
        }

        private void moeda1_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner moeda1 = (Spinner)sender;
            TextView hdnMoeda1 = FindViewById<TextView>(Resource.Id.hdnMoeda1);

            var item = moeda1.GetItemAtPosition(e.Position);
            long posicao = moeda1.GetItemIdAtPosition(e.Position);
            hdnMoeda1.Text = (Resources.GetStringArray(Resource.Array.cotacaocompra_array))[posicao];
        }

        private void moeda2_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner moeda2 = (Spinner)sender;
            TextView hdnMoeda2 = FindViewById<TextView>(Resource.Id.hdnMoeda2);

            var item = moeda2.GetItemAtPosition(e.Position);
            long posicao = moeda2.GetItemIdAtPosition(e.Position);
            hdnMoeda2.Text = (Resources.GetStringArray(Resource.Array.cotacaocompra_array))[posicao];
        }

        private void btnConverter_Click(object sender, System.EventArgs e)
        {
            Spinner moeda1 = FindViewById<Spinner>(Resource.Id.moeda1);
            Spinner moeda2 = FindViewById<Spinner>(Resource.Id.moeda2);
            TextView hdnMoeda1 = FindViewById<TextView>(Resource.Id.hdnMoeda1);
            TextView hdnMoeda2 = FindViewById<TextView>(Resource.Id.hdnMoeda2);
            EditText txtQuantidade = FindViewById<EditText>(Resource.Id.txtQuantidade);
            TextView lblResultado = FindViewById<TextView>(Resource.Id.lblResultado);
            bool valid = true;

            if (txtQuantidade.Text.Length == 0) {
                Toast.MakeText(this, "Por favor informe a quantidade desejada para a conversão", ToastLength.Short).Show();
                valid = false;
            }

            if (moeda1.GetItemIdAtPosition(moeda1.SelectedItemPosition) == 0) {
                Toast.MakeText(this, "Por favor selecione uma moeda de origem", ToastLength.Short).Show();
                valid = false;
            }

            if (moeda2.GetItemIdAtPosition(moeda2.SelectedItemPosition) == 0) {
                Toast.MakeText(this, "Por favor selecione uma moeda de destino", ToastLength.Short).Show();
                valid = false;
            }

            if (moeda1.GetItemIdAtPosition(moeda1.SelectedItemPosition) == moeda2.GetItemIdAtPosition(moeda2.SelectedItemPosition)) {
                Toast.MakeText(this, "A moeda de origem e destino devem ser diferentes", ToastLength.Short).Show();
                valid = false;
            }

            if(valid)
            {

                try
                {
                    Uri uri = new Uri(string.Format(UrlApi, hdnMoeda1.Text, hdnMoeda2.Text));
                    WebRequest request = WebRequest.Create(uri);
                    request.ContentType = "application/json";
                    request.Method = "GET";

                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                            Toast.MakeText(this, "Erro na conexão com o serviço, tente novamente", ToastLength.Short);

                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            var content = reader.ReadToEnd();
                            if (string.IsNullOrWhiteSpace(content))
                            {
                                Toast.MakeText(this, "Nenhum conteúdo encontrado", ToastLength.Short);
                            }
                            else
                            {
                                JObject jObject = JObject.Parse(content);
                                decimal taxaCambio = (decimal)jObject["query"]["results"]["row"]["col1"];
                                lblResultado.Text = (Convert.ToDecimal(txtQuantidade.Text.Replace(".", ",")) * taxaCambio).ToString("0.##");
                            }
                        }
                    }
                }

                catch (Exception)
                {
                    Toast.MakeText(this, "Ocorreu um erro, por favor tente novamente mais tarde", ToastLength.Short).Show();
                }

                finally
                {
                    //Desaloca os objetos da memória
                    moeda1 = null;
                    moeda2 = null;
                    hdnMoeda1 = null;
                    hdnMoeda2 = null;
                    txtQuantidade = null;
                    lblResultado = null;
                }
            }
        }
    }
}

