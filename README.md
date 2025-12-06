# 👻 Yurei v13 - Manual de Usuario

Yurei es una herramienta de análisis forense y seguridad diseñada para detectar modificaciones no autorizadas, exploits, configuraciones ocultas (FFlags) y actividad sospechosa en el sistema.

Este manual detalla cómo utilizar cada una de las funciones de la herramienta.

## 🖥️ Interfaz Principal (Dashboard)

Al iniciar Yurei, verás el panel principal con un resumen del estado del sistema:
- **Contadores en Tiempo Real**: Muestra la cantidad de FFlags, Exploits y registros de navegador encontrados.
- **Tarjetas de Funciones**: Accesos directos a las herramientas principales.
- **Monitoreo Activo**: El sistema realiza un escaneo superficial cada 10 segundos automáticamente.

---

## 🔍 Módulos y Funciones

### 1. 🕵️ Forense (Lookups)
Esta es la función principal para realizar un escaneo completo y generar reportes.

*   **Cómo usarlo:**
    1.  Ve a la pestaña **Lookups** (icono de lupa).
    2.  Selecciona las casillas de lo que deseas escanear:
        *   `FFlags`: Configuraciones modificadas de Roblox.
        *   `Exploits`: Programas de inyección de código (Synapse, Krnl, etc.).
        *   `Browser History`: Historial de búsqueda y visitas relacionadas con cheats.
    3.  Haz clic en el botón **"Ejecutar Escaneo Forense"**.
    4.  Espera a que termine la barra de progreso.
    5.  **Resultados**: Verás una lista detallada con la ubicación y descripción de cada hallazgo.
    6.  **Reporte**: Si tienes configurado un Webhook de Discord, se enviará un reporte automático con archivos de log adjuntos.

### 2. ⚡ Escáner de Exploits
Detecta software malicioso o herramientas de inyección de código (executors).

*   **Funciones:**
    *   **Buscar Exploits**: Escanea procesos activos, carpetas de descargas, AppData y el registro de Windows en busca de firmas conocidas (Wave, Solara, Velocity, etc.).
    *   **Actualizar WEAO**: Descarga la lista más reciente de exploits conocidos desde la base de datos de WEAO.xyz para mantener la detección al día.

### 3. ⚙️ Análisis de FFlags
Busca configuraciones "Fast Flags" de Roblox que suelen usarse para alterar el comportamiento del juego (ver a través de paredes, volar, etc.).

*   **Funciones:**
    *   **Escanear FFlags**: Busca archivos `.json` o configuraciones en el registro que modifiquen el cliente de Roblox.
    *   **Monitoreo 24/7**: Activa un escaneo automático cada 5 minutos para detectar cambios persistentes.

### 4. 🔒 Red y VPN
Detecta si el usuario está utilizando VPNs, Proxies o herramientas para ocultar su IP.

*   **Funciones:**
    *   **Escanear VPN**: Busca procesos de VPN conocidos (NordVPN, ProtonVPN, etc.) y adaptadores de red virtuales.
    *   **Analizar Puertos**: Verifica conexiones activas sospechosas.

### 5. 🧹 Herramientas (Tools)
Utilidades para limpieza y mantenimiento.

*   **Deep Clean (Limpieza Profunda)**:
    *   ⚠️ **Advertencia**: Esta función elimina permanentemente archivos.
    *   Borra exploits detectados.
    *   Elimina configuraciones FFlag.
    *   Limpia el historial de navegación relacionado con cheats.
    *   Úsalo para sanitizar un sistema infectado o comprometido.

---

## ⚙️ Configuración

En la pestaña de **Settings** (Configuración), puedes personalizar:
*   **Discord Webhook**: Pega aquí la URL de tu Webhook de Discord para recibir los reportes de los escaneos de forma remota.
*   **Tema**: Cambiar entre modo claro/oscuro (si está disponible).

---

## 🚀 Instalación y Ejecución

1.  Descarga la última versión.
2.  Extrae el archivo `.zip`.
3.  Ejecuta `Yurei.exe` como Administrador (recomendado para acceso completo al sistema).
4.  Si compilas desde el código fuente:
    ```bash
    dotnet build
    cd bin/Debug/net8.0-windows/
    ./Yurei.exe
    ```

---

## ⚠️ Nota Legal

Yurei está diseñado exclusivamente para **fines educativos y de análisis forense**. El uso de esta herramienta para espiar o dañar sistemas ajenos sin autorización es ilegal. Los desarrolladores no se hacen responsables del mal uso de este software.
