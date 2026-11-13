import { useState } from 'react'

/** Redondea hacia arriba a un número "limpio" (1, 2, 2.5, 5 × 10^n) para el eje. */
function techoLimpio(n) {
  if (n <= 0) return 1
  const base = 10 ** Math.floor(Math.log10(n))
  return [1, 2, 2.5, 5, 10].map((m) => m * base).find((v) => v >= n)
}

/**
 * Gráfico de columnas simple (una sola serie), sin librerías.
 * datos: [{ clave, etiqueta, etiquetaCorta?, valor, detalle }] (etiquetaCorta: la que se ve en pantallas chicas)
 * formato: (n) => texto del valor; formatoEje: idem compacto.
 * Al pasar el mouse (o con Tab) muestra el valor; "Ver tabla" muestra los mismos datos sin gráfico.
 */
export default function GraficoBarras({ datos, formato, formatoEje = formato, nombreValor, alto = 200, densa = false }) {
  const [activo, setActivo] = useState(null)
  const [verTabla, setVerTabla] = useState(false)

  const maximo = Math.max(0, ...datos.map((d) => d.valor))
  const techo = techoLimpio(maximo)
  const indicePico = maximo > 0 ? datos.findIndex((d) => d.valor === maximo) : -1
  const marcas = [techo, techo / 2, 0]

  return (
    <div className="grafico">
      <div className="grafico-opciones">
        <button className="btn-enlace" onClick={() => setVerTabla(!verTabla)}>{verTabla ? 'Ver gráfico' : 'Ver tabla'}</button>
      </div>

      {verTabla ? (
        <table className="tabla-compacta">
          <thead><tr><th></th><th className="num">{nombreValor}</th><th className="num">Detalle</th></tr></thead>
          <tbody>
            {datos.map((d) => (
              <tr key={d.clave}><td>{d.etiqueta}</td><td className="num">{formato(d.valor)}</td><td className="num texto-suave">{d.detalle}</td></tr>
            ))}
          </tbody>
        </table>
      ) : (
        <div className="grafico-cuerpo" style={{ height: alto }}>
          <div className="grafico-eje" aria-hidden="true">
            {marcas.map((m) => <span key={m}>{formatoEje(m)}</span>)}
          </div>
          <div className="grafico-area" onMouseLeave={() => setActivo(null)}>
            {marcas.map((m) => <div key={m} className="grafico-linea" style={{ bottom: `${(m / techo) * 100}%` }} />)}
            {datos.map((d, i) => {
              const altura = (d.valor / techo) * 100
              return (
                <div key={d.clave} className={`grafico-columna ${activo === i ? 'activa' : ''}`} tabIndex={0}
                     aria-label={`${d.etiqueta}: ${formato(d.valor)}${d.detalle ? `, ${d.detalle}` : ''}`}
                     onMouseEnter={() => setActivo(i)} onFocus={() => setActivo(i)} onBlur={() => setActivo(null)}>
                  {i === indicePico && activo !== i && (
                    <span className="grafico-valor" style={{ bottom: `calc(${altura}% + 4px)` }}>{formatoEje(d.valor)}</span>
                  )}
                  {d.valor > 0 && <div className="grafico-barra" style={{ height: `${altura}%` }} />}
                  {activo === i && (
                    <div className="grafico-tooltip" style={{ bottom: `calc(${altura}% + 8px)` }}>
                      <strong>{formato(d.valor)}</strong>
                      <span>{d.etiqueta}{d.detalle ? ` · ${d.detalle}` : ''}</span>
                    </div>
                  )}
                </div>
              )
            })}
          </div>
          <div />
          <div className={`grafico-etiquetas ${densa ? 'densa' : ''}`} aria-hidden="true">
            {datos.map((d) => (
              <span key={d.clave}>
                <span className="etiqueta-larga">{d.etiqueta}</span>
                <span className="etiqueta-corta">{d.etiquetaCorta ?? d.etiqueta}</span>
              </span>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
