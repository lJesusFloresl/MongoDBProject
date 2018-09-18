namespace MongoDBProject
{
    /// <summary>
    /// Clase utilizada para manipular las respuestas de mongo
    /// </summary>
    public class MongoResponse
    {
        public bool exito { get; set; }
        public string mensaje { get; set; }

        public MongoResponse() { }

        public MongoResponse(bool exito, string mensaje)
        {
            this.exito = exito;
            this.mensaje = mensaje;
        }
    }
}
