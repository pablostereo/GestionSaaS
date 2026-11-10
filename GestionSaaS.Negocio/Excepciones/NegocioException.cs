namespace GestionSaaS.Negocio.Excepciones;

/// <summary>Regla de negocio no cumplida (la Api la devuelve como 400).</summary>
public class NegocioException(string mensaje) : Exception(mensaje);

/// <summary>El registro pedido no existe para el cliente actual (la Api la devuelve como 404).</summary>
public class NoEncontradoException(string mensaje) : Exception(mensaje);

/// <summary>Credenciales inválidas o usuario bloqueado (la Api la devuelve como 401).</summary>
public class NoAutorizadoException(string mensaje) : Exception(mensaje);
