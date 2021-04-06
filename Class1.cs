using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Core.Utils;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace RealisticSwim
{
    public class Class1 : RocketPlugin
    {
        public Class1 Instance;
        public List<CSteamID> Nadadores { get; set; }
        public Timer Reloj { get; set; }
        protected override void Load()
        {
            Instance = this;
            Nadadores = new List<CSteamID>();

            U.Events.OnPlayerConnected += Conectado;
            U.Events.OnPlayerDisconnected += Desconectado;


            Reloj = new Timer(1 * 1000); Reloj.Elapsed += Reloj_Elapsed;
            Reloj.Start();
            Reloj.AutoReset = true;
            Reloj.Enabled = true;

        }

        private void Reloj_Elapsed(object sender, ElapsedEventArgs e)
        {
            TaskDispatcher.QueueOnMainThread(() =>
            {
                Provider.clients.ForEach(delegate (SteamPlayer client)
                {

                    UnturnedPlayer playeR;
                    playeR = UnturnedPlayer.FromSteamPlayer(client);
                    bool flag = Nadadores.Contains(playeR.CSteamID);

                    if (!flag)
                    {
                        Savedata valor = playeR.GetComponent<Savedata>();
                        if(valor.tiempo <= 3)
                        {
                            playeR.Damage(100, playeR.Position, EDeathCause.BREATH, ELimb.SKULL, playeR.CSteamID);
                        }
                        else
                        {
                            valor.tiempo--;
                        }
                    }

                });
            });
        }

        private void Desconectado(UnturnedPlayer player)
        {
            if (player.HasPermission("realistic.swim"))
            {
                Nadadores.Remove(player.CSteamID);
            }
        }

        private void Conectado(UnturnedPlayer player)
        {
            if (player.HasPermission("realistic.swim"))
            {
                Nadadores.Add(player.CSteamID);
            }
        }

        public void FixedUpdate()
        {
            Provider.clients.ForEach(delegate (SteamPlayer client) {

                UnturnedPlayer player;
                player = UnturnedPlayer.FromSteamPlayer(client);


                bool flag = Nadadores.Contains(player.CSteamID);
                Savedata valor = player.GetComponent<Savedata>();
                if (!flag)
                {
                    if (player.Player.stance.isBodyUnderwater)
                    {
                        

                        if(valor.tiempo <= 16)
                        {
                            if (valor.sonido_activo)
                            {
                                return;
                            }
                            else
                            {
                                valor.sonido_activo = true;
                                EffectManager.sendEffect(45001, 15, player.Position);
                            }
                        }
                        else if(valor.tiempo <= 3)
                        {
                            player.Damage(100, player.Position, EDeathCause.BREATH, ELimb.SKULL, player.CSteamID);

                        }
                    }
                    else
                    {
                        valor.tiempo = 30;
                        valor.sonido_activo = false;
                        EffectManager.askEffectClearByID(45001, player.CSteamID);
                    }
                }
            });
        }

        protected override void Unload()
        {
            base.Unload();
        }
    }
}
