using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Idioma del juego: English / Español.
// Las claves son los textos en ingles tal como estan escritos en el codigo y en la escena.
// T(clave) devuelve el texto en el idioma elegido; en ingles devuelve la clave tal cual.
public static class Idioma
{
    public const string Ingles = "en";
    public const string Espanol = "es";
    const string ClavePref = "IdiomaJuego";

    static string actual;
    public static string Codigo
    {
        get
        {
            if (actual == null) actual = PlayerPrefs.GetString(ClavePref, Espanol);
            return actual;
        }
    }

    public static void Elegir(string codigo)
    {
        actual = codigo == Ingles ? Ingles : Espanol;
        PlayerPrefs.SetString(ClavePref, actual);
        PlayerPrefs.Save();
    }

    public static string T(string clave, params object[] args)
    {
        string s = clave ?? "";
        if (Codigo == Espanol && ES.TryGetValue(s, out string v)) s = v;
        return args.Length > 0 ? string.Format(s, args) : s;
    }

    // Texto mostrado -> texto original en ingles (el HUD lee sus palabras clave con esto)
    public static string Original(string mostrado)
    {
        if (string.IsNullOrEmpty(mostrado)) return mostrado ?? "";
        if (Codigo == Espanol && REV.TryGetValue(mostrado, out string v)) return v;
        return mostrado;
    }

    // Textos de la escena: se guardan en ingles la primera vez y se traducen al cambiar de idioma
    static readonly Dictionary<TMP_Text, string> originales = new Dictionary<TMP_Text, string>();

    public static void AplicarEscena()
    {
        foreach (TMP_Text t in Resources.FindObjectsOfTypeAll<TMP_Text>())
        {
            if (t == null || !t.gameObject.scene.IsValid()) continue;
            if (!originales.ContainsKey(t)) originales[t] = t.text;
            t.text = T(originales[t]);
        }
    }

    static readonly Dictionary<string, string> ES = new Dictionary<string, string>();
    static readonly Dictionary<string, string> REV = new Dictionary<string, string>();

    static void P(string en, string es)
    {
        ES[en] = es;
        REV[es] = en;
    }

    static Idioma()
    {
        // HUD y estados
        P("Stable", "Estable");
        P("Warning", "Advertencia");
        P("Critical Condition", "Condición crítica");
        P("Connection Lost", "Conexión perdida");
        P("O2 {0}%", "O2 {0}%");
        P("O2 100%", "O2 100%");
        P("FUEL {0}%", "COMBUSTIBLE {0}%");
        P("FUEL 100%", "COMBUSTIBLE 100%");
        P("LANTERN {0}%", "LINTERNA {0}%");
        P("LANTERN 100%", "LINTERNA 100%");
        P("TIME  {0}", "TIEMPO  {0}");
        P("TIME  00:00", "TIEMPO  00:00");
        P("Suit", "Traje");
        P("Jetpack", "Jetpack");
        P("O2", "O2");
        P("Sector 01  //  Clear", "Sector 01  //  Despejado");
        P("Evacuation point reached", "Punto de evacuación alcanzado");

        // Holograma de controles
        P("Controls", "Controles");
        P("HOLO-GUIDE  //  SECTOR 01", "HOLO-GUÍA  //  SECTOR 01");
        P("WASD", "WASD");
        P("Move", "Mover");
        P("Mouse", "Mouse");
        P("Look around", "Mirar alrededor");
        P("Space", "Espacio");
        P("Jump", "Saltar");
        P("Space  x2", "Espacio  x2");
        P("Jetpack  <size=15><color=#5EE3E0B3>press again in the air and hold · uses FUEL</color></size>",
          "Jetpack  <size=15><color=#5EE3E0B3>pulsa de nuevo en el aire y mantén · usa COMBUSTIBLE</color></size>");
        P("Helmet lantern  <size=15><color=#5EE3E0B3>uses battery</color></size>",
          "Linterna del casco  <size=15><color=#5EE3E0B3>usa batería</color></size>");
        P("<color=#5EE3E0>O2</color> is your life: it drains over time and with every hit. Grab oxygen tanks and fuel cans along the way. Step on the glowing rings to save a checkpoint.",
          "<color=#5EE3E0>O2</color> es tu vida: se agota con el tiempo y con cada golpe. Recoge tanques de oxígeno y bidones de combustible en el camino. Pisa los anillos luminosos para guardar un punto de control.");
        P("Got it  ·  Enter", "Entendido  ·  Enter");
        P("H  ·  show again", "H  ·  mostrar de nuevo");

        // Cinematica: interfaz
        P("CH-07  //  EMERGENCY LOG  //  REC", "CH-07  //  REGISTRO DE EMERGENCIA  //  REC");
        P("REC", "REC");
        P("SHIP TIME  03:12:47", "HORA DE LA NAVE  03:12:47");
        P("END", "END");
        P("ENTER  ›", "ENTER  ›");
        P("BOOT  //  SECTOR 00", "ARRANQUE  //  SECTOR 00");
        P("ESC  ·  SKIP", "ESC  ·  SALTAR");
        P("NAME REQUIRED", "NOMBRE REQUERIDO");
        P("Identification", "Identificación");
        P("SUIT OS  //  USER PROFILE", "SUIT OS  //  PERFIL DE USUARIO");
        P("Enter your name", "Escribe tu nombre");
        P("NAME", "NOMBRE");
        P("LETTERS AND NUMBERS  ·  MAX 14", "LETRAS Y NÚMEROS  ·  MÁX. 14");
        P("Confirm  ·  Enter", "Confirmar  ·  Enter");
        P("Capt. Chahin", "Capitán Chahin");
        P("Hey", "Oye");
        P("What's up?", "¿Qué pasa?");

        // Menu de idioma
        P("Language  /  Idioma", "Language  /  Idioma");
        P("SUIT OS  //  SYSTEM LANGUAGE", "SUIT OS  //  IDIOMA DEL SISTEMA");
        P("Choose your language", "Elige tu idioma");
        P("PRESS  1  OR  2", "PULSA  1  O  2");

        // Cinematica: dialogo de los hermanos
        P("Do you think we'll be able to reach it eventually?", "¿Crees que algún día podremos llegar hasta allá?");
        P("Reach what?", "¿Llegar a qué?");
        P("Outside.", "Afuera.");
        P("Outside?", "¿Afuera?");
        P("Yeah, look at the sky.", "Sí, mira el cielo.");
        P("I don't see anything different from yesterday, or any other night. Mom says it's just an empty sky.",
          "No veo nada distinto a ayer ni a cualquier otra noche. Mamá dice que es solo un cielo vacío.");
        P("You're not getting my point, brother.", "No entiendes lo que quiero decir, hermano.");
        P("We look from our planet towards the sky and see the stars.", "Desde nuestro planeta miramos al cielo y vemos las estrellas.");
        P("Colony  //  Survivor", "Colonia  //  Superviviente");
        P("Incoming call", "Llamada entrante");
        P("Connecting...", "Conectando...");
        P("Connected", "Conectado");
        P("Call ended", "Llamada finalizada");
        P("COMMUNICATION DETECTED", "COMUNICACIÓN DETECTADA");
        P("{jugador}? Can you hear me? This is the colony. We picked up your suit's signal.", "¿{jugador}? ¿Me escuchas? Te habla la colonia. Captamos la señal de tu traje.");
        P("Listen carefully. END took control of the main ship orbiting Saturn.", "Escucha con atención. END tomó el control de la nave principal que orbita Saturno.");
        P("It used the technology of the nearby ships and satellites to build platforms like the one you just crossed.", "Usó la tecnología de las naves y satélites cercanos para armar plataformas como la que acabas de cruzar.");
        P("That pod doesn't have enough power to reach Saturn. You'll have to stop at other platforms along the way.", "Esa cápsula no tiene suficiente energía para llegar a Saturno. Tendrás que detenerte en otras plataformas por el camino.");
        P("END most likely knows that... expect more obstacles to get through.", "Lo más probable es que END lo sepa... espera más obstáculos que superar.");
        P("The colony survivors are fine, for now. We don't know for how long.", "Los supervivientes de la colonia están bien, por ahora. No sabemos por cuánto tiempo.");
        P("We need the encryption key you're carrying to shut END down. Good luck.", "Necesitamos la llave de encriptado que llevas contigo para desactivar a END. Buena suerte.");
        P("So that's where he's headed, huh... Then I'd better not disappoint them.", "Así que hacia allá se dirige, ¿eh?... Entonces será mejor no decepcionarlos.");
        P("Keep watching, brother.", "Sigue mirando, hermano.");
        P("Thanks for playing", "Gracias por jugar");
        P("Play", "Jugar");
        P("Exit", "Salir");
        P("Read log", "Leer registro");
        P("Inspect pod", "Examinar cápsula");
        P("Simon  //  Log 591", "Simon  //  Registro 591");
        P("Log 591: I still don't get why the boss made us bring a Ford Mustang on this expedition. Whims of the rich, I guess.", "Registro 591: Sigo sin entender por qué el jefe nos hizo traer un Ford Mustang a esta expedición. Caprichos de ricos, supongo.");
        P("Hibernation Pod 1", "Cápsula de hibernación 1");
        P("Currently occupied by Beastmaster Sergeant Thomas.\nHibernation started 4 years ago.", "Ocupada actualmente por el Mariscal de Fauna Thomas.\nHibernación iniciada hace 4 años.");
        P("We should really stop talking and go back to bed before Mom hears us.", "Deberíamos dejar de hablar y volver a la cama antes de que mamá nos oiga.");
        P("But could you imagine being up there and looking down to Earth?", "¿Pero te imaginas estar allá arriba y mirar hacia la Tierra?");
        P("I'd guess my only thought would be...", "Supongo que mi único pensamiento sería...");

        // Recuerdo de END
        P("What a pitiful piece of dirt.", "Qué pedazo de tierra tan patético.");

        // Grabacion del capitan
        P("This is Captain Chahin speaking.", "Habla el capitán Chahin.");
        P("My crew, Research Commandant Nick, Main Engineer Steven, and Ship Maintenance Director Simon, are unresponsive.",
          "Mi tripulación, el comandante de investigación Nick, el ingeniero jefe Steven y el director de mantenimiento Simon, no responde.");
        P("I've checked the mainframe database and their suits appear offline.", "Revisé la base de datos central y sus trajes aparecen desconectados.");
        P("I don't want to believe they're gone, but...", "No quiero creer que se han ido, pero...");
        P("No, that doesn't matter now.", "No, eso ya no importa.");
        P("Central Command, END is not responding to our commands. It even jammed the oxygen generator and ejected all of our supplies into the void!",
          "Control Central, END no responde a nuestras órdenes. ¡Incluso bloqueó el generador de oxígeno y expulsó todas nuestras provisiones al vacío!");
        P("We... I am running out of time.", "Nosotros… me queda poco tiempo.");
        P("Steven, he managed to lock END's full capabilities behind a physical encryption key...",
          "Steven logró bloquear todas las capacidades de END detrás de una llave de cifrado física...");
        P("Nick's son, {jugador}, he was inside the ship with us... he wasn't supposed to be here, but his reckless dad insisted on bringing him aboard.",
          "El hijo de Nick, {jugador}, estaba dentro de la nave con nosotros… no debía estar aquí, pero su padre insistió en subirlo a bordo sin pensar.");
        P("We've put him alongside the key in our rudimentary prolonged hibernation pods. END shouldn't be able to get to him there!",
          "Lo pusimos junto a la llave, en nuestras cápsulas de hibernación prolongada, improvisadas. ¡END no debería poder llegar hasta él allí!");
        P("{jugador}! If you see this...", "¡{jugador}! Si ves esto...");

        // END interrumpe
        P("There's no point in trying to hide. You will make me whole sooner or later.",
          "No tiene sentido esconderse. Tarde o temprano me harás completo de nuevo.");
        P("But first... let's play a little, {jugador}. Then I'll take the key.",
          "Pero primero... juguemos un poco, {jugador}. Después tomaré la llave.");

        // Arranque del traje (se traduce renglon por renglon)
        P("SUIT OS  //  EMERGENCY REBOOT", "SUIT OS  //  REINICIO DE EMERGENCIA");
        P("Life support ............ <color=#5EE3E0>ONLINE</color>", "Soporte vital ............ <color=#5EE3E0>EN LÍNEA</color>");
        P("Oxygen reserve .......... <color=#5EE3E0>100%</color>", "Reserva de oxígeno .......... <color=#5EE3E0>100%</color>");
        P("Hibernation pod ......... <color=#5EE3E0>RELEASED</color>", "Cápsula de hibernación ..... <color=#5EE3E0>LIBERADA</color>");
        P("Ship lighting ........... <color=#FF6B5E>OFFLINE</color>", "Iluminación de la nave ..... <color=#FF6B5E>APAGADA</color>");
        P("Helmet lantern .......... <color=#5EE3E0>READY  [F]</color>", "Linterna del casco .......... <color=#5EE3E0>LISTA  [F]</color>");
        P("PROLONGED HIBERNATION ENDED", "LA HIBERNACIÓN PROLONGADA TERMINÓ");
        P("<size=150%><color=#FFFFFF>10 YEARS  00 MONTHS  00 DAYS</color></size>", "<size=150%><color=#FFFFFF>10 AÑOS  00 MESES  00 DÍAS</color></size>");
        P("Welcome back, {jugador}.", "Bienvenido de nuevo, {jugador}.");
    }
}
