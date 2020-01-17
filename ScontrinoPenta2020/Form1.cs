﻿using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace ScontrinoPenta
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private static string userdb = "";
        private static string passdb = "";
        private static string mct = "";
        private static string ditron = "";
        private static string epson = "";
        private static string percorsodb = "";
        private static string ipdb = "";
        private static string postazione = "";
        private static string percorsomultidriver = "";
        private static string percorsowinecr = "";
        private static string percorsofpmate = "";
        private static string dettnsco = "";
        private static string dettcapi = "";
        private static string scontrinoparlante = "";
        public static bool connopen = false;
        public static FbConnection connection;
        public static bool servizio = false;
        public static Log Log = new Log();

        private void Form1_Load(object sender, EventArgs e)
        {
            Log.InizializzareLog();
            Log.WriteLog("----------------------------------------------------------------------------");
            Log.WriteLog("Avvio Scontrino Penta");
            LoadIni();
            OpenDatabase();
            AggDatabase();

            servizio = true;
            FbRemoteEvent scontrino = new FbRemoteEvent(connection);
            if (postazione == "1")
            {
                scontrino.AddEvents(new string[] { "NEW_SCONTRINOIMMEDIATO_P1" });
            }
            else if (postazione == "2")
            {
                scontrino.AddEvents(new string[] { "NEW_SCONTRINOIMMEDIATO_P2" });
            }
            else
            {
                MessageBox.Show("Errore numero postazione. Modificare PentaStart.ini","Scontrino Penta");
                Environment.Exit(0);
            }            

            scontrino.RemoteEventCounts += new FbRemoteEventEventHandler(EventCounts);
            scontrino.QueueEvents();
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.ShowBalloonTip(1000, "PentaStart - Scontrino Penta", "Servizio Attivo", ToolTipIcon.Info);
            Log.WriteLog("Servizio ScontrinoPenta attivo");
        }

        private void GetPostazioneOperatori()
        {
            string qtypostazioni = "";
            while (qtypostazioni != "1" && qtypostazioni != "2")
            {
                qtypostazioni = Interaction.InputBox("Inserire quantita di postazioni (Max 2)", "Scontrino Penta", "1", -1, -1);
                if (qtypostazioni == "2")
                {
                    string operatoripos1 = Interaction.InputBox("Inserire ID operatori per la postazione 1 (separati da virgola (','). Esempi: 1,2,3 )", "Scontrino Penta", "1", -1, -1);

                    string[] operatorip1 = operatoripos1.Split(',');
                    string ORoperatoripos1 = "(";
                    foreach (string operatore in operatorip1)
                    {
                        ORoperatoripos1 = ORoperatoripos1 + " NEW.DOCUMENTOLOTTOOPERATOREID = " + operatore + " OR ";
                    }
                    ORoperatoripos1 = ORoperatoripos1.Substring(0, ORoperatoripos1.Length - 3) + ")";

                    string operatoripos2 = Interaction.InputBox("Inserire ID operatori per la postazione 2 (separati da virgola (','). Esempi: 4,5,6 )", "Scontrino Penta", "1", -1, -1);
                    string[] operatorip2 = operatoripos2.Split(',');
                    string ORoperatoripos2 = "(";
                    foreach (string operatore in operatorip2)
                    {
                        ORoperatoripos2 = ORoperatoripos2 + " NEW.DOCUMENTOLOTTOOPERATOREID = " + operatore + " OR ";
                    }
                    ORoperatoripos2 = ORoperatoripos2.Substring(0, ORoperatoripos2.Length - 3) + ")";

                    FbCommand sql4 = new FbCommand("CREATE TRIGGER NEW_SCONTRINOIMMEDIATO FOR DOCUMENTILOTTI ACTIVE AFTER INSERT POSITION 0 AS BEGIN IF(NEW.TIPODOCUMENTOID = '4' AND " + ORoperatoripos1 + ") THEN BEGIN POST_EVENT 'NEW_SCONTRINOIMMEDIATO_P1'; END IF(NEW.TIPODOCUMENTOID = '4' AND " + ORoperatoripos2 + ") THEN BEGIN POST_EVENT 'NEW_SCONTRINOIMMEDIATO_P2'; END END;", connection);
                    sql4.ExecuteNonQuery();
                }
                else if (qtypostazioni == "1")
                {
                    FbCommand sql4 = new FbCommand("CREATE TRIGGER NEW_SCONTRINOIMMEDIATO FOR DOCUMENTILOTTI ACTIVE AFTER INSERT POSITION 0 AS BEGIN IF(NEW.TIPODOCUMENTOID = '4') THEN BEGIN POST_EVENT 'NEW_SCONTRINOIMMEDIATO_P1'; END END;", connection);
                    sql4.ExecuteNonQuery();
                }

                if (postazione != "1" && postazione != "2" && qtypostazioni != "1")
                {
                    string pos = "";
                    pos = Interaction.InputBox("Inserire il numero della postazione (1 o 2)", "Scontrino Penta");
                    while (pos != "1" && pos != "2")
                    {
                        pos = Interaction.InputBox("Inserire il numero della postazione (1 o 2)", "Scontrino Penta");
                    }
                    ModificaIni("STAMPANTI", "Postazione", pos);
                    postazione = pos;
                }
                else
                {
                    ModificaIni("STAMPANTI", "Postazione", "1");
                    postazione = "1";
                }
            }
        }


        private void AggDatabase()
        {
            int i = 0;

            FbCommand trigg_newscon = new FbCommand("SELECT COUNT(RDB$RELATION_NAME) FROM RDB$TRIGGERS WHERE RDB$SYSTEM_FLAG = 0 AND RDB$TRIGGER_NAME='NEW_SCONTRINOIMMEDIATO';", connection);
            if (Convert.ToInt16(trigg_newscon.ExecuteScalar()) == 0)
            {
                Log.WriteLog("Avvio configurazione Postazione");
                GetPostazioneOperatori();
                i++;
            }

            if (i > 0)
            {
                ImpostaPagamenti();
                MessageBox.Show("Database aggiornato. Riavvio Scontrino Penta. Query:" + i.ToString(), "PentaStart");
                Log.WriteLog("Database aggiornato - Query: " + i);
                Application.Restart();
            }
                
        }

        private void EventCounts(object sender, FbRemoteEventEventArgs args)
        {
            Log.WriteLog("Nuovo scontrino rilevato (Postazione " + postazione + ")");
            ScontrinoImmediato();
        }

        private void ScontrinoImmediato()
        {
            StampaScontrino attendere = new StampaScontrino();
            attendere.Show();
            attendere.Refresh();
            DateTime Inizio = DateTime.Now;
            List<string> FbLotti = LetturaLotti();
            List<string> Lottirighe = LetturaLottirighe();

            if (Lottirighe.Count > 0 && FbLotti.Count == 0)
            {
                FbLotti = LetturaLotti();
            }

            Decimal TotaleDocumento = 0.00M;
            decimal PagamentoContante = 0.00M;
            decimal PagamentoCarta = 0.00M;
            decimal PagamentoNonRiscosso = 0.00M;

            GetPagamenti(FbLotti, ref PagamentoContante, ref PagamentoCarta, ref PagamentoNonRiscosso);
            decimal TotalePagamento = GetTotalePagamento(PagamentoContante, PagamentoCarta, PagamentoNonRiscosso);

            FbCommand readarticoli = new FbCommand("SELECT LR.ARTICOLOID, LR.LOTTORIGADESCRIZIONE, ART.ARTICOLOCODREP, LAV.LOTTORIGALAVORAZIONEPREZZO,MLR.MODIFICATOREID ,MLR.MODIFLOTTORIGAVALORE FROM LOTTIRIGHE LR JOIN ARTICOLI ART ON LR.ARTICOLOID = ART.ARTICOLOID JOIN LOTTIRIGHELAVORAZIONI LAV ON LR.LOTTORIGAID = LAV.LOTTORIGAID FULL JOIN MODIFICATORILOTTIRIGHE MLR ON LR.LOTTORIGAID = MLR.LOTTORIGAID WHERE LR.LOTTORIGAID IN(" + string.Join(",", Lottirighe) + ");", connection);
            FbDataReader reader1 = readarticoli.ExecuteReader();
            List<FbArticolo> Articoli = new List<FbArticolo>();
            while (reader1.Read())
            {
                int tipovariazione = 0;
                double variazioneprezzo = 0;
                try
                {
                    if (Int32.TryParse(reader1.GetString(4), out tipovariazione))
                    {
                        if (tipovariazione == 1 || tipovariazione == 7)
                        {
                            if (Double.TryParse(reader1.GetString(5), out variazioneprezzo))
                                variazioneprezzo = (reader1.GetDouble(3) * (-variazioneprezzo)) / 100;
                            variazioneprezzo = -variazioneprezzo;
                        }
                        else if (tipovariazione == 2)
                        {
                            if (Double.TryParse(reader1.GetString(5), out variazioneprezzo))
                                variazioneprezzo = (reader1.GetDouble(3) * (variazioneprezzo)) / 100;
                        }
                        else if (tipovariazione == 5 || tipovariazione == 6)
                        {
                            Double.TryParse(reader1.GetString(5), out variazioneprezzo);
                        }
                        else if (tipovariazione == 4 || tipovariazione == 8 || tipovariazione == 3)
                        {
                            variazioneprezzo = -reader1.GetDouble(3);
                        }
                    }
                }
                catch (Exception)
                {
                    variazioneprezzo = 0;
                }
                Articoli.Add(new FbArticolo { ARTICOLOID = reader1.GetInt32(0), LOTTORIGADESCRIZIONE = reader1.GetString(1), ARTICOLOCODREP = reader1.GetInt32(2), LOTTORIGALAVORAZIONEPREZZO = reader1.GetDecimal(3), MODIFLOTTORIGAVALORE = variazioneprezzo });
            }
            reader1.Close();

            double TotaleSconti = 0.00;
            List<ElementsScontrino> ScontrinoRow = new List<ElementsScontrino>();
            List<Articolo> articolidesc = new List<Articolo>();
            List<Articolo> alreadyread = new List<Articolo>();
            foreach (FbArticolo articolo in Articoli)
            {
                try
                {
                    TotaleSconti = TotaleSconti + GetDouble(articolo.MODIFLOTTORIGAVALORE);
                }
                catch (Exception)
                {

                }
                try
                {
                    TotaleDocumento = TotaleDocumento + GetDecimal(articolo.LOTTORIGALAVORAZIONEPREZZO) + Convert.ToDecimal(articolo.MODIFLOTTORIGAVALORE);
                }
                catch (Exception)
                {
                    try
                    {
                        TotaleDocumento = TotaleDocumento + GetDecimal(articolo.LOTTORIGALAVORAZIONEPREZZO);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Errore: " + ex.ToString());
                    }
                }

                if (articolo.ARTICOLOCODREP == 0)
                {
                    articolo.ARTICOLOCODREP = 1;
                }

                articolidesc.Add(new Articolo { rep = articolo.ARTICOLOCODREP.ToString(), desc = articolo.LOTTORIGADESCRIZIONE, prezzo = GetDecimal(articolo.LOTTORIGALAVORAZIONEPREZZO) });
            }

            FbCommand readscontrini = new FbCommand("SELECT LO.LOTTONUMERO,MLO.TIPOMODIFICATOREID,MLO.MODIFLOTTOVALORE FROM LOTTI LO FULL JOIN MODIFICATORILOTTI MLO ON LO.LOTTOID = MLO.LOTTOID WHERE LO.LOTTOID IN(" + string.Join(",", FbLotti) + ") ORDER BY LO.LOTTOID;", connection);
            FbDataReader reader2 = readscontrini.ExecuteReader();
            List<FbScontrini> Lotti = new List<FbScontrini>();
            while (reader2.Read())
            {
                int tipovariazione = 0;
                double variazioneprezzo = 0;
                try
                {
                    if (Int32.TryParse(reader2.GetString(1), out tipovariazione))
                    {
                        if (tipovariazione == 1 || tipovariazione == 7)
                        {
                            if (Double.TryParse(TotaleDocumento.ToString(), out variazioneprezzo))
                                variazioneprezzo = (reader2.GetDouble(2) * (-variazioneprezzo)) / 100;
                            variazioneprezzo = -variazioneprezzo;
                        }
                        else if (tipovariazione == 2)
                        {
                            if (Double.TryParse(TotaleDocumento.ToString(), out variazioneprezzo))
                                variazioneprezzo = (reader2.GetDouble(2) * (variazioneprezzo)) / 100;
                        }
                        else if (tipovariazione == 5 || tipovariazione == 6)
                        {
                            Double.TryParse(reader1.GetString(5), out variazioneprezzo);
                        }
                        else if (tipovariazione == 4 || tipovariazione == 8 || tipovariazione == 3)
                        {
                            variazioneprezzo = -Convert.ToDouble(TotaleDocumento);
                        }
                    }
                }
                catch (Exception)
                {
                    variazioneprezzo = 0;
                }
                TotaleSconti = TotaleSconti + GetDouble(variazioneprezzo);

                TotaleDocumento = TotaleDocumento + Convert.ToDecimal(variazioneprezzo);

                Lotti.Add(new FbScontrini { LOTTONUMERO = reader2.GetInt32(0), MODIFLOTTOVALORE = variazioneprezzo });
            }
            reader2.Close();
            Log.WriteLog("Totale sconti: €" + TotaleSconti);
            Log.WriteLog("Totale scontrino: €" + TotaleDocumento);

            Log.WriteLog("Scontrini:" + string.Join(" - ",Lotti.Select(x => x.LOTTONUMERO).ToArray()));

            int contatore = 0;
            foreach (var item in articolidesc)
            {
                if (alreadyread.Any(x => x.desc == item.desc && x.prezzo == item.prezzo && x.rep == item.rep))
                {
                    contatore = 0;
                }
                else
                {
                    alreadyread.Add(new Articolo { rep = item.rep, desc = item.desc, prezzo = item.prezzo });
                    foreach (var item2 in articolidesc)
                    {
                        if (item2.desc == item.desc && item2.prezzo == item.prezzo && item2.rep == item.rep)
                        {
                            contatore++;
                        }
                    }
                    if (item.prezzo != 0)
                    {
                        ScontrinoRow.Add(new ElementsScontrino { rep = item.rep, qty = contatore, desc = item.desc, prezzo = item.prezzo });
                        Log.WriteLog("Nuova riga scontrino: " + contatore + " " + item.desc + " €" + (item.prezzo * contatore).ToString());
                    }
                    contatore = 0;
                }
            }
            contatore = 0;
            Log.WriteLog("Totale righe: " + ScontrinoRow.Count);
            Log.WriteLog("Totale pezzi: " + ScontrinoRow.Sum(pezzi => pezzi.qty));

            string infocliente = ""; /*GetInfoCliente();*/
            Log.WriteLog("Info Cliente: " + infocliente);

            if (TotalePagamento == 0)
            {
                Log.WriteLog("Totale Pagamento: 0. Ricerca pagamenti precedenti in corso...");
                GetPagamentiAnticipati(FbLotti, ref PagamentoContante, ref PagamentoCarta, ref PagamentoNonRiscosso);
                TotalePagamento = GetTotalePagamento(PagamentoContante, PagamentoCarta, PagamentoNonRiscosso);
                if (TotalePagamento == 0)
                {
                    AutoClosingMessageBox.Show("Errore calcolo pagamento. Controllare impostazione pagamenti.", "Scontrino Penta", 3000, MessageBoxButtons.OK);
                    Log.WriteLog("Errore calcolo pagamento. Valori Pagamenti = 0");
                    TotaleDocumento = 0;
                    TotalePagamento = 0;
                    TotaleSconti = 0;
                    PagamentoContante = 0;
                    PagamentoCarta = 0;
                    PagamentoNonRiscosso = 0;
                    attendere.Close();
                    return;
                }
            }

            ControlloDriverInvioScontrino();

            if (TotaleDocumento > TotalePagamento)
            {
                FbCommand isrecupero = new FbCommand("SELECT COUNT(LOTTOID) FROM DOCUMENTILOTTI WHERE TIPODOCUMENTOID = 4 AND LOTTOID IN (" + string.Join(" - ", FbLotti) + ")", connection);
                int qtylotti = Convert.ToInt32(isrecupero.ExecuteScalar()) / FbLotti.Count;
                if (qtylotti == 1)
                {
                    Log.WriteLog("Rilevato scontrino acconto");
                    if (mct == "true")
                    {
                        Log.WriteLog("Avvio invio scontrino acconto RCH");
                        InvioScontrinoAccontoRCH(ScontrinoRow, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                        Log.WriteLog("Fine invio scontrino acconto RCH");
                    }
                    else if (ditron == "true")
                    {
                        Log.WriteLog("Avvio invio scontrino acconto Ditron");
                        InvioScontrinoAccontoDitron(ScontrinoRow, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                        Log.WriteLog("Fine invio scontrino acconto Ditron");
                    }
                    else if (epson == "true")
                    {
                        Log.WriteLog("Avvio invio scontrino acconto Epson");
                        InvioScontrinoAccontoEpson(ScontrinoRow, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                        Log.WriteLog("Fine invio scontrino acconto Epson");
                    }
                }
                else if (qtylotti >= 2)
                {
                    Log.WriteLog("Rilevato scontrino recupero credito");
                    if (mct == "true")
                    {
                        Log.WriteLog("Avvio invio scontrino recupero credito RCH");
                        InvioScontrinoRecuperoRCH(ScontrinoRow, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                        Log.WriteLog("Fine invio scontrino recupero credito RCH");
                    }
                    else if (ditron == "true")
                    {
                        Log.WriteLog("Avvio invio scontrino recupero credito Ditron");
                        InvioScontrinoRecuperoDitron(ScontrinoRow, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                        Log.WriteLog("Fine invio scontrino recupero credito Ditron");
                    }
                    else if (epson == "true")
                    {
                        Log.WriteLog("Avvio invio scontrino recupero credito Epson");
                        InvioScontrinoRecuperoEpson(ScontrinoRow, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                        Log.WriteLog("Fine invio scontrino recupero credito Epson");
                    }
                }
                else if (qtylotti == 0)
                {
                    Log.WriteLog("Rilevato scontrino pago anticipato");
                    Log.WriteLog("Calcolo pagamanti in corso");
                    GetPagamentiAnticipati(FbLotti, ref PagamentoContante, ref PagamentoCarta, ref PagamentoNonRiscosso);
                    if (mct == "true")
                    {
                        Log.WriteLog("Avvio invio scontrino pago anticipato RCH");
                        InvioScontrinoRCH(ScontrinoRow, TotaleSconti, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                        Log.WriteLog("Fine invio scontrino pago anticipato RCH");
                    }
                    else if (ditron == "true")
                    {
                        Log.WriteLog("Avvio invio scontrino pago anticipato Ditron");
                        InvioScontrinoDitron(ScontrinoRow, TotaleSconti, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                        Log.WriteLog("Fine invio scontrino pago anticipato Ditron");
                    }
                    else if (epson == "true")
                    {
                        Log.WriteLog("Avvio invio scontrino pago anticipato Epson");
                        InvioScontrinoEpson(ScontrinoRow, TotaleSconti, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                        Log.WriteLog("Fine invio scontrino pago anticipato Epson");
                    }
                }
            }
            else if (TotaleDocumento <= TotalePagamento)
            {
                Log.WriteLog("Rilevato scontrino completo");
                if (mct == "true")
                {
                    Log.WriteLog("Avvio invio scontrino completo RCH");
                    InvioScontrinoRCH(ScontrinoRow, TotaleSconti, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                    Log.WriteLog("Fine invio scontrino completo RCH");
                }
                else if (ditron == "true")
                {
                    Log.WriteLog("Avvio invio scontrino completo Ditron");
                    InvioScontrinoDitron(ScontrinoRow, TotaleSconti, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                    Log.WriteLog("Fine invio scontrino completo Ditron");
                }
                else if (epson == "true")
                {
                    Log.WriteLog("Avvio invio scontrino completo Epson");
                    InvioScontrinoEpson(ScontrinoRow, TotaleSconti, PagamentoContante, PagamentoCarta, PagamentoNonRiscosso, Lotti, infocliente);
                    Log.WriteLog("Fine invio scontrino completo Epson");
                }
            }

            Log.WriteLog("Fine scrittura file scontrino (" + DateTime.Now.Subtract(Inizio).TotalSeconds + " secondi)");
            //ControlloFile();
            attendere.Close();

            TotaleDocumento = 0;
            TotalePagamento = 0;
            TotaleSconti = 0;
            PagamentoContante = 0;
            PagamentoCarta = 0;
            PagamentoNonRiscosso = 0;
        }

        private void ControlloFile()
        {
            string filescontrino = GetPercorsoStampante() + "\\scontrino.txt";
            int secondi = 0;
            while (File.Exists(filescontrino))
            {
                System.Threading.Thread.Sleep(1000);
                secondi++;
                if (secondi > 12)
                {
                    
                }
            }
        }

        private string GetPercorsoStampante()
        {
            if (mct == "true")
            {
                return percorsomultidriver;
            }
            else if (ditron == "true")
            {
                return percorsowinecr;
            }
            else if (epson == "true")
            {
                return percorsofpmate;
            }
            else
            {
                Log.WriteLog("Errore percorso stampante");
                return "";
            }
        }

        private static void ControlloDriverInvioScontrino()
        {
            if (mct == "true")
            {
                Process[] multidriver = Process.GetProcessesByName("MULTIDRIVER_SERVER");
                if (multidriver.Length == 0)
                {
                    System.Diagnostics.Process multiserver = new System.Diagnostics.Process();
                    multiserver.StartInfo.FileName = percorsomultidriver + "\\MULTIDRIVER_SERVER.exe";
                    multiserver.Start();
                }
            }
            else if (ditron == "true")
            {
                Process[] multidriver = Process.GetProcessesByName("SoEcrCom");
                if (multidriver.Length == 0)
                {
                    System.Diagnostics.Process multiserver = new System.Diagnostics.Process();
                    multiserver.StartInfo.FileName = percorsowinecr + "\\Drivers\\SoEcrCom.exe";
                    multiserver.Start();
                }
            }
            else if (epson == "true")
            {
                Process[] multidriver = Process.GetProcessesByName("EpsonFpMate");
                if (multidriver.Length == 0)
                {
                    System.Diagnostics.Process multiserver = new System.Diagnostics.Process();
                    multiserver.StartInfo.FileName = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\EpsonFpMate\\EpsonFpMate.exe";
                    multiserver.Start();
                }
            }
        }

        private static string GetInfoCliente()
        {
            string infocliente = "";
            FbCommand getInfoCliente = new FbCommand("SELECT INFOCLIENTE FROM SCONTRINO_CODFIS1", connection);
            try
            {
                infocliente = getInfoCliente.ExecuteScalar().ToString();
                Log.WriteLog("Codice fiscale/P.IVA Cliente: " + infocliente);
            }
            catch (Exception)
            {
                infocliente = "";
            }

            return infocliente;
        }

        private static decimal GetTotalePagamento(decimal PagamentoContante, decimal PagamentoCarta, decimal PagamentoNonRiscosso)
        {
            decimal TotalePagamento = PagamentoContante + PagamentoCarta + PagamentoNonRiscosso;
            Log.WriteLog("Totale Pagamento: €" + TotalePagamento.ToString());
            return TotalePagamento;
        }

        private static void GetPagamenti(List<string> FbLotti, ref decimal PagamentoContante, ref decimal PagamentoCarta, ref decimal PagamentoNonRiscosso)
        {
            FbCommand getPagamentoContanti = new FbCommand("select SUM(pagamentolottoversato) from pagamentilotti where pagamentilotti.TIPOPAGAMENTOID in (select TIPIPAGAMENTI.TIPOPAGAMENTOID from TIPIPAGAMENTI where TIPIPAGAMENTI.TIPOPAGAMENTOCODICE = 1) and PAGAMENTILOTTI.LOTTOID in (" + string.Join(",", FbLotti) + ") and pagamentolottodata = \'" + DateTime.Now.ToString("yyyy-MM-dd") + "\' and pagamentolottoora = (select max(pagamentolottoora) from pagamentilotti where pagamentolottodata = \'" + DateTime.Now.ToString("yyyy-MM-dd") + "\')", connection);
            var pagContanti = getPagamentoContanti.ExecuteScalar();
            if (!Convert.IsDBNull(pagContanti))
            {
                PagamentoContante = Convert.ToDecimal(pagContanti);
                Log.WriteLog("Pagamento Contanti: €" + PagamentoContante.ToString());
            }

            FbCommand getPagamentoElettronico = new FbCommand("select SUM(pagamentolottoversato) from pagamentilotti where pagamentilotti.TIPOPAGAMENTOID in (select TIPIPAGAMENTI.TIPOPAGAMENTOID from TIPIPAGAMENTI where TIPIPAGAMENTI.TIPOPAGAMENTOCODICE = 2) and PAGAMENTILOTTI.LOTTOID in (" + string.Join(",", FbLotti) + ") and pagamentolottodata = \'" + DateTime.Now.ToString("yyyy-MM-dd") + "\' and pagamentolottoora = (select max(pagamentolottoora) from pagamentilotti where pagamentolottodata = \'" + DateTime.Now.ToString("yyyy-MM-dd") + "\')", connection);
            var pagElettronico = getPagamentoElettronico.ExecuteScalar();
            if (!Convert.IsDBNull(pagElettronico))
            {
                PagamentoCarta = Convert.ToDecimal(pagElettronico);
                Log.WriteLog("Pagamento Elettronico: €" + PagamentoCarta.ToString());
            }

            FbCommand getPagamentoNonRiscosso = new FbCommand("select SUM(pagamentolottoversato) from pagamentilotti where pagamentilotti.TIPOPAGAMENTOID in (select TIPIPAGAMENTI.TIPOPAGAMENTOID from TIPIPAGAMENTI where TIPIPAGAMENTI.TIPOPAGAMENTOCODICE = 3) and PAGAMENTILOTTI.LOTTOID in (" + string.Join(",", FbLotti) + ") and pagamentolottodata = \'" + DateTime.Now.ToString("yyyy-MM-dd") + "\' and pagamentolottoora = (select max(pagamentolottoora) from pagamentilotti where pagamentolottodata = \'" + DateTime.Now.ToString("yyyy-MM-dd") + "\')", connection);
            var pagNonRiscosso = getPagamentoNonRiscosso.ExecuteScalar();
            if (!Convert.IsDBNull(pagNonRiscosso))
            {
                PagamentoNonRiscosso = Convert.ToDecimal(pagNonRiscosso);
                Log.WriteLog("Pagamento Non Riscosso: €" + PagamentoNonRiscosso.ToString());
            }
        }

        private static void GetPagamentiAnticipati(List<string> FbLotti, ref decimal PagamentoContante, ref decimal PagamentoCarta, ref decimal PagamentoNonRiscosso)
        {
            FbCommand getPagamentoContanti = new FbCommand("select SUM(pagamentolottoversato) from pagamentilotti where pagamentilotti.TIPOPAGAMENTOID in (select TIPIPAGAMENTI.TIPOPAGAMENTOID from TIPIPAGAMENTI where TIPIPAGAMENTI.TIPOPAGAMENTOCODICE = 1) and PAGAMENTILOTTI.LOTTOID in (" + string.Join(",", FbLotti) + ")", connection);
            var pagContanti = getPagamentoContanti.ExecuteScalar();
            if (!Convert.IsDBNull(pagContanti))
            {
                PagamentoContante = Convert.ToDecimal(pagContanti);
                Log.WriteLog("Pagamento Contanti: €" + PagamentoContante.ToString());
            }

            FbCommand getPagamentoElettronico = new FbCommand("select SUM(pagamentolottoversato) from pagamentilotti where pagamentilotti.TIPOPAGAMENTOID in (select TIPIPAGAMENTI.TIPOPAGAMENTOID from TIPIPAGAMENTI where TIPIPAGAMENTI.TIPOPAGAMENTOCODICE = 2) and PAGAMENTILOTTI.LOTTOID in (" + string.Join(",", FbLotti) + ")", connection);
            var pagElettronico = getPagamentoElettronico.ExecuteScalar();
            if (!Convert.IsDBNull(pagElettronico))
            {
                PagamentoCarta = Convert.ToDecimal(pagElettronico);
                Log.WriteLog("Pagamento Elettronico: €" + PagamentoCarta.ToString());
            }

            FbCommand getPagamentoNonRiscosso = new FbCommand("select SUM(pagamentolottoversato) from pagamentilotti where pagamentilotti.TIPOPAGAMENTOID in (select TIPIPAGAMENTI.TIPOPAGAMENTOID from TIPIPAGAMENTI where TIPIPAGAMENTI.TIPOPAGAMENTOCODICE = 3) and PAGAMENTILOTTI.LOTTOID in (" + string.Join(",", FbLotti) + ")", connection);
            var pagNonRiscosso = getPagamentoNonRiscosso.ExecuteScalar();
            if (!Convert.IsDBNull(pagNonRiscosso))
            {
                PagamentoNonRiscosso = Convert.ToDecimal(pagNonRiscosso);
                Log.WriteLog("Pagamento Non Riscosso: €" + PagamentoNonRiscosso.ToString());
            }
        }

        private static List<string> LetturaLottirighe()
        {
            List<string> Lottirighe = new List<string>();
            FbCommand commandLottirighe = new FbCommand("SELECT LOTTORIGAID from DOCUMENTIRIGHE where DOCUMENTIRIGHE.DOCUMENTORIGADATA = \'" + DateTime.Now.ToString("yyyy-MM-dd") + "\' AND DOCUMENTIRIGHE.DOCUMENTORIGAORA = (SELECT MAX(DOCUMENTORIGAORA) FROM DOCUMENTIRIGHE WHERE DOCUMENTORIGADATA = \'" + DateTime.Now.ToString("yyyy-MM-dd") + "\' and TIPODOCUMENTOID = 4) and TIPODOCUMENTOID = 4", connection);
            FbDataReader readerLottirighe = commandLottirighe.ExecuteReader();
            while (readerLottirighe.Read())
            {
                Lottirighe.Add(readerLottirighe.GetString(0));
            }
            if (Lottirighe.Count < 30)
            {
                Log.WriteLog("Lottirighe: " + string.Join(" - ", Lottirighe));
            }
            return Lottirighe;
        }

        private static List<string> LetturaLotti()
        {
            List<string> FbLotti = new List<string>();
            FbCommand commandLotti = new FbCommand("select lottoid from documentilotti where documentilotti.DOCUMENTOLOTTODATA = \'" + DateTime.Now.ToString("yyyy-MM-dd") + "\' AND DOCUMENTILOTTI.DOCUMENTOLOTTOORA = (SELECT MAX(DOCUMENTOLOTTOORA) FROM DOCUMENTILOTTI WHERE DOCUMENTOLOTTODATA = \'" + DateTime.Now.ToString("yyyy-MM-dd") + "\' and TIPODOCUMENTOID = 4) and TIPODOCUMENTOID = 4", connection);
            try
            {
                FbDataReader readerLotti = commandLotti.ExecuteReader();
                while (readerLotti.Read())
                {
                    FbLotti.Add(readerLotti.GetString(0));
                }
                if (FbLotti.Count == 0)
                {
                    Log.WriteLog("Nessun Lotto trovato");
                }
                else
                {
                    Log.WriteLog("Lotti: " + string.Join(" - ", FbLotti));
                }
                return FbLotti;
            }
            catch (Exception ex)
            {
                Log.WriteLog("Errore ricerca numero lotti");
                Log.WriteLog("Errore: " + ex.ToString());
                return FbLotti;
            }
        }

        private decimal GetDecimal(decimal? dec)
        {
            if (dec == null)
                return 0;
            else
                return (decimal)dec;
        }

        private double GetDouble(double? dou)
        {
            if (dou == null)
                return 0;
            else
                return (double)dou;
        }

        private void InvioScontrinoAccontoDitron(List<ElementsScontrino> ScontrinoRow,decimal PagamentoContante, decimal PagamentoCarta, decimal PagamentoNonRiscosso,List<FbScontrini> listasco, string infoscontrino)
        {
            List<string> commandi = new List<string>();
            commandi.Add("clear");
            commandi.Add("chiave reg");

            decimal TotalePagamento = PagamentoContante + PagamentoCarta + PagamentoNonRiscosso;

            if (infoscontrino != "" && scontrinoparlante == "true")
            {
                commandi.Add("INP TERM=61");
                commandi.Add("INP ALFA=\'" + infoscontrino + "\',TERM=49");
            }

            commandi.Add("vend rep=1, pre=" + TotalePagamento.ToString("0.00").Replace(",", ".") + ", des=\'ACCONTO\'");
            if (dettcapi == "true")
            {
                foreach (ElementsScontrino item in ScontrinoRow)
                {
                    commandi.Add("prmsg riga = \'  " + item.qty.ToString() + " " + item.desc.Replace("*", "").Replace("\'", "") + "\'");
                }
            }
            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                commandi.Add("alleg on");
            }
            if (PagamentoContante > 0)
            {
                commandi.Add("chius t=1, imp=" + PagamentoContante.ToString("0.00").Replace(",", "."));
            }
            if (PagamentoCarta > 0)
            {
                commandi.Add("chius t=5, imp=" + PagamentoCarta.ToString("0.00").Replace(",", "."));
            }
            if (PagamentoNonRiscosso > 0)
            {
                commandi.Add("chius t=2, imp=" + PagamentoNonRiscosso.ToString("0.00").Replace(",", "."));
            }

            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                if (listasco.Count == 1)
                {
                    commandi.Add("alleg riga=\'Rif. Scontrino N. " + listasco[0].LOTTONUMERO + "\'");
                    commandi.Add("alleg fine");
                }
                else
                {
                    commandi.Add("alleg riga=\'Rif. Scontrini\'");
                    foreach (FbScontrini nscontrino in listasco)
                    {
                        commandi.Add("alleg riga=\'N. " + nscontrino.LOTTONUMERO + "\'");
                    }
                    commandi.Add("alleg fine");
                }
            }

            if (PagamentoContante == 0.00M && PagamentoCarta == 0.00M && PagamentoNonRiscosso == 0.00M)
            {
                commandi.Clear();
                MessageBox.Show("Errore stampa scontrino." +
                    "Errore calcolo pagamento", "PentaStart");
                return;
            }

            //MessageBox.Show(Path.Combine(percorsowinecr, "\\TOSEND\\scontrino.txt"));
            using (StreamWriter outputFile = new StreamWriter(percorsowinecr + "\\TOSEND\\scontrino.txt"))
            {
                foreach (string line in commandi)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        private void InvioScontrinoAccontoRCH(List<ElementsScontrino> ScontrinoRow,decimal PagamentoContante, decimal PagamentoCarta, decimal PagamentoNonRiscosso, List<FbScontrini> listasco, string infoscontrino)
        {
            List<string> commandi = new List<string>();
            commandi.Add("=K");
            commandi.Add("=C1");
            
            decimal TotalePagamento = PagamentoContante + PagamentoCarta + PagamentoNonRiscosso;

            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                if (listasco.Count == 1)
                {
                    commandi.Add("=\"/?A/(Rif. Scontrino N. " + listasco[0].LOTTONUMERO + ")");
                }
                else
                {
                    commandi.Add("=\"/?A/(Rif. Scontrini)");
                    foreach (FbScontrini nscontrino in listasco)
                    {
                        commandi.Add("=\"/?A/(N. " + nscontrino.LOTTONUMERO + ")");
                    }
                }
            }

            commandi.Add("=R1/$" + TotalePagamento.ToString().Replace(",", "") + "/(ACCONTO)");
            if (dettcapi == "true")
            {
                foreach (ElementsScontrino item in ScontrinoRow)
                {

                    commandi.Add("=\"/&1/(  " + item.qty.ToString() + " " + item.desc.Replace("*", "").Replace("\'", "") + ")");
                }
            }

            if (infoscontrino != "" && scontrinoparlante == "true")
            {
                commandi.Add("=\"/?C/(" + infoscontrino + ")");
            }

            if (PagamentoContante > 0)
            {
                commandi.Add("=T1/$" + PagamentoContante.ToString("0.00").Replace(",", ""));

            }
            if (PagamentoCarta > 0)
            {
                commandi.Add("=T3/$" + PagamentoCarta.ToString("0.00").Replace(",", ""));
            }
            if (PagamentoNonRiscosso > 0)
            {
                commandi.Add("=T2/$" + PagamentoNonRiscosso.ToString("0.00").Replace(",", ""));
            }


            if (PagamentoContante == 0.00M && PagamentoCarta == 0.00M && PagamentoNonRiscosso == 0.00M)
            {
                commandi.Clear();
                MessageBox.Show("Errore stampa scontrino." +
                    "Errore calcolo pagamento", "PentaStart");
                return;
            }

            //MessageBox.Show(Path.Combine(percorsowinecr, "\\TOSEND\\scontrino.txt"));
            using (StreamWriter outputFile = new StreamWriter(percorsomultidriver + "\\TOSEND\\scontrino.txt"))
            {
                foreach (string line in commandi)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        private void InvioScontrinoAccontoEpson(List<ElementsScontrino> ScontrinoRow, decimal PagamentoContante, decimal PagamentoCarta, decimal PagamentoNonRiscosso, List<FbScontrini> listasco, string infoscontrino)
        {
            List<string> commandi = new List<string>();
            commandi.Add("clear");
            commandi.Add("chiave reg");

            decimal TotalePagamento = PagamentoContante + PagamentoCarta + PagamentoNonRiscosso;

            if (infoscontrino != "" && scontrinoparlante == "true")
            {
                commandi.Add("INP TERM=61");
                commandi.Add("INP ALFA=\'" + infoscontrino + "\',TERM=49");
            }

            commandi.Add("vend rep=1, prezzo=" + TotalePagamento.ToString("0.00").Replace(",", ".") + ", des=\'ACCONTO\'");

            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                if (listasco.Count == 1)
                {
                    commandi.Add("PRMSG LINE=\'Rif. Scontrino N. " + listasco[0].LOTTONUMERO + "\'");
                }
                else
                {
                    commandi.Add("PRMSG LINE=\'Rif. Scontrini\'");
                    foreach (FbScontrini nscontrino in listasco)
                    {
                        commandi.Add("PRMSG LINE=\'N. " + nscontrino.LOTTONUMERO + "\'");
                    }
                }
            }

            if (dettcapi == "true")
            {
                foreach (ElementsScontrino item in ScontrinoRow)
                {
                    commandi.Add("prmsg riga = \'  " + item.qty.ToString() + " " + item.desc.Replace("*", "").Replace("\'", "") + "\'");
                }
            }

            if (PagamentoContante > 0)
            {
                commandi.Add("chius t=1, imp=" + PagamentoContante.ToString("0.00").Replace(",", "."));
            }
            if (PagamentoCarta > 0)
            {
                commandi.Add("chius t=5, imp=" + PagamentoCarta.ToString("0.00").Replace(",", "."));
            }
            if (PagamentoNonRiscosso > 0)
            {
                commandi.Add("chius t=2, imp=" + PagamentoNonRiscosso.ToString("0.00").Replace(",", "."));
            }

            if (PagamentoContante == 0.00M && PagamentoCarta == 0.00M && PagamentoNonRiscosso == 0.00M)
            {
                commandi.Clear();
                MessageBox.Show("Errore stampa scontrino." +
                    "Errore calcolo pagamento", "PentaStart");
                return;
            }

            //MessageBox.Show(Path.Combine(percorsowinecr, "\\TOSEND\\scontrino.txt"));
            using (StreamWriter outputFile = new StreamWriter(percorsofpmate + "\\TOSEND\\scontrino.txt"))
            {
                foreach (string line in commandi)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        private void InvioScontrinoRecuperoDitron(List<ElementsScontrino> ScontrinoRow, decimal PagamentoContante, decimal PagamentoCarta, decimal PagamentoNonRiscosso,List<FbScontrini> listasco, string infoscontrino)
        {
            List<string> commandi = new List<string>();
            commandi.Add("clear");
            commandi.Add("chiave reg");


            decimal TotalePagamento = PagamentoContante + PagamentoCarta + PagamentoNonRiscosso;

            if (infoscontrino != "" && scontrinoparlante == "true")
            {
                commandi.Add("INP TERM=61");
                commandi.Add("INP ALFA=\'" + infoscontrino + "\',TERM=49");
            }

            commandi.Add("vend rep=1, pre=" + TotalePagamento.ToString("0.00").Replace(",", ".") + ", des=\'RECUPERO CREDITO\'");

            if (dettcapi == "true")
            {
                foreach (ElementsScontrino item in ScontrinoRow)
                {
                    commandi.Add("prmsg riga = \'  " + item.qty.ToString() + " " + item.desc.Replace("*", "").Replace("\'", "") + "\'");
                }
            }

            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                commandi.Add("alleg on");
            }

            if (PagamentoContante > 0)
            {
                commandi.Add("chius t=1, imp=" + PagamentoContante.ToString("0.00").Replace(",", "."));
            }
            if (PagamentoCarta > 0)
            {
                commandi.Add("chius t=5, imp=" + PagamentoCarta.ToString("0.00").Replace(",", "."));
            }
            if (PagamentoNonRiscosso > 0)
            {
                commandi.Add("chius t=2, imp=" + PagamentoNonRiscosso.ToString("0.00").Replace(",", "."));
            }

            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                if (listasco.Count == 1)
                {
                    commandi.Add("alleg riga=\'Rif. Scontrino N. " + listasco[0].LOTTONUMERO + "\'");
                    commandi.Add("alleg fine");
                }
                else
                {
                    commandi.Add("alleg riga=\'Rif. Scontrini\'");
                    foreach (FbScontrini nscontrino in listasco)
                    {
                        commandi.Add("alleg riga=\'N. " + nscontrino.LOTTONUMERO + "\'");
                    }
                    commandi.Add("alleg fine");
                }
            }

            if (PagamentoContante == 0.00M && PagamentoCarta == 0.00M && PagamentoNonRiscosso == 0.00M)
            {
                commandi.Clear();
                MessageBox.Show("Errore stampa scontrino." +
                    "Errore calcolo pagamento", "PentaStart");
                return;
            }
            //MessageBox.Show(Path.Combine(percorsowinecr, "\\TOSEND\\scontrino.txt"));
            using (StreamWriter outputFile = new StreamWriter(percorsowinecr + "\\TOSEND\\scontrino.txt"))
            {
                foreach (string line in commandi)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        private void InvioScontrinoRecuperoRCH(List<ElementsScontrino> ScontrinoRow,decimal PagamentoContante, decimal PagamentoCarta, decimal PagamentoNonRiscosso, List<FbScontrini> listasco, string infoscontrino)
        {
            List<string> commandi = new List<string>();
            commandi.Add("=K");
            commandi.Add("=C1");


            decimal TotalePagamento = PagamentoContante + PagamentoCarta + PagamentoNonRiscosso;

            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                if (listasco.Count == 1)
                {
                    commandi.Add("=\"/?A/(Rif. Scontrino N. " + listasco[0].LOTTONUMERO + ")");
                }
                else
                {
                    commandi.Add("=\"/?A/(Rif. Scontrini)");
                    foreach (FbScontrini nscontrino in listasco)
                    {
                        commandi.Add("=\"/?A/(N. " + nscontrino.LOTTONUMERO + ")");
                    }
                }
            }

            commandi.Add("=R1/$" + TotalePagamento.ToString("0.00").Replace(",", "") + "/(RECUPERO CREDITO)");

            if (dettcapi == "true")
            {
                foreach (ElementsScontrino item in ScontrinoRow)
                {
                    commandi.Add("=\"/&1/(  " + item.qty.ToString() + " " + item.desc.Replace("*", "").Replace("\'", "") + ")");
                }
            }

            if (infoscontrino != "" && scontrinoparlante == "true")
            {
                commandi.Add("=\"/?C/(" + infoscontrino + ")");
            }

            if (PagamentoContante > 0)
            {
                commandi.Add("=T1/$" + PagamentoContante.ToString("0.00").Replace(",", ""));

            }
            if (PagamentoCarta > 0)
            {
                commandi.Add("=T3/$" + PagamentoCarta.ToString("0.00").Replace(",", ""));
            }
            if (PagamentoNonRiscosso > 0)
            {
                commandi.Add("=T2/$" + PagamentoNonRiscosso.ToString("0.00").Replace(",", ""));
            }

            if (PagamentoContante == 0.00M && PagamentoCarta == 0.00M && PagamentoNonRiscosso == 0.00M)
            {
                commandi.Clear();
                MessageBox.Show("Errore stampa scontrino." +
                    "Errore calcolo pagamento", "PentaStart");
                return;
            }
            //MessageBox.Show(Path.Combine(percorsowinecr, "\\TOSEND\\scontrino.txt"));
            using (StreamWriter outputFile = new StreamWriter(percorsomultidriver + "\\TOSEND\\scontrino.txt"))
            {
                foreach (string line in commandi)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        private void InvioScontrinoRecuperoEpson(List<ElementsScontrino> ScontrinoRow, decimal PagamentoContante, decimal PagamentoCarta, decimal PagamentoNonRiscosso, List<FbScontrini> listasco, string infoscontrino)
        {
            List<string> commandi = new List<string>();
            commandi.Add("clear");
            commandi.Add("chiave reg");


            decimal TotalePagamento = PagamentoContante + PagamentoCarta + PagamentoNonRiscosso;

            if (infoscontrino != "" && scontrinoparlante == "true")
            {
                commandi.Add("INP TERM=61");
                commandi.Add("INP ALFA=\'" + infoscontrino + "\',TERM=49");
            }

            commandi.Add("vend rep=1, prezzo=" + TotalePagamento.ToString("0.00").Replace(",", ".") + ", des=\'RECUPERO CREDITO\'");

            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                if (listasco.Count == 1)
                {
                    commandi.Add("PRMSG LINE=\'Rif. Scontrino N. " + listasco[0].LOTTONUMERO + "\'");
                }
                else
                {
                    commandi.Add("PRMSG LINE=\'Rif. Scontrini\'");
                    foreach (FbScontrini nscontrino in listasco)
                    {
                        commandi.Add("PRMSG LINE=\'N. " + nscontrino.LOTTONUMERO + "\'");
                    }
                }
            }

            if (dettcapi == "true")
            {
                foreach (ElementsScontrino item in ScontrinoRow)
                {
                    commandi.Add("prmsg riga = \'  " + item.qty.ToString() + " " + item.desc.Replace("*", "").Replace("\'", "") + "\'");
                }
            }

            if (PagamentoContante > 0)
            {
                commandi.Add("chius t=1, imp=" + PagamentoContante.ToString("0.00").Replace(",", "."));
            }
            if (PagamentoCarta > 0)
            {
                commandi.Add("chius t=5, imp=" + PagamentoCarta.ToString("0.00").Replace(",", "."));
            }
            if (PagamentoNonRiscosso > 0)
            {
                commandi.Add("chius t=2, imp=" + PagamentoNonRiscosso.ToString("0.00").Replace(",", "."));
            }


            if (PagamentoContante == 0.00M && PagamentoCarta == 0.00M && PagamentoNonRiscosso == 0.00M)
            {
                commandi.Clear();
                MessageBox.Show("Errore stampa scontrino." +
                    "Errore calcolo pagamento", "PentaStart");
                return;
            }
            //MessageBox.Show(Path.Combine(percorsowinecr, "\\TOSEND\\scontrino.txt"));
            using (StreamWriter outputFile = new StreamWriter(percorsofpmate + "\\TOSEND\\scontrino.txt"))
            {
                foreach (string line in commandi)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        private void InvioScontrinoDitron(List<ElementsScontrino> ScontrinoRow, double TotaleSconti, decimal PagamentoContante, decimal PagamentoCarta, decimal PagamentoNonRiscosso,List<FbScontrini> listasco, string infoscontrino)
        {
            List<string> commandi = new List<string>();
            commandi.Add("clear");
            commandi.Add("chiave reg");

            if (infoscontrino != "" && scontrinoparlante == "true")
            {
                commandi.Add("INP TERM=61");
                commandi.Add("INP ALFA=\'" + infoscontrino + "\',TERM=49");
            }

            foreach (ElementsScontrino item in ScontrinoRow)
            {
                commandi.Add("vend rep=" + item.rep + ", pre=" + (item.prezzo * item.qty).ToString("0.00").Replace(",", ".") + ", des=\'" + item.qty.ToString() + " " + item.desc.Replace("*", "").Replace("\'", "") + "\'");
            }

            if (TotaleSconti > 0)
            {
                commandi.Add("subt");
                commandi.Add("inp num = " + TotaleSconti.ToString("0.00").Replace(",", ".") + ", term = 34");
            }
            else if (TotaleSconti < 0)
            {
                commandi.Add("Sconto val=" + TotaleSconti.ToString("0.00").Replace(",", ".").Replace("-", "") + ",subtot");
            }

            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                commandi.Add("alleg on");
            }

            if (PagamentoContante > 0)
            {
                commandi.Add("chius t=1, imp=" + PagamentoContante.ToString("0.00").Replace(",", "."));

            }
            if (PagamentoCarta > 0)
            {
                commandi.Add("chius t=5, imp=" + PagamentoCarta.ToString("0.00").Replace(",", "."));
            }
            if (PagamentoNonRiscosso > 0)
            {
                commandi.Add("chius t=2, imp=" + PagamentoNonRiscosso.ToString("0.00").Replace(",", "."));
            }

            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                if (listasco.Count == 1)
                {
                    commandi.Add("alleg riga=\'Rif. Scontrino N. " + listasco[0].LOTTONUMERO + "\'");
                    commandi.Add("alleg fine");
                }
                else
                {
                    commandi.Add("alleg riga=\'Rif. Scontrini\'");
                    foreach (FbScontrini nscontrino in listasco)
                    {
                        commandi.Add("alleg riga=\'N. " + nscontrino.LOTTONUMERO + "\'");
                    }
                    commandi.Add("alleg fine");
                }
            }

            if (PagamentoContante == 0.00M && PagamentoCarta == 0.00M && PagamentoNonRiscosso == 0.00M)
            {
                commandi.Clear();
                MessageBox.Show("Errore stampa scontrino." +
                    "Errore calcolo pagamento", "PentaStart");
                return;
            }
            //MessageBox.Show(Path.Combine(percorsowinecr, "\\TOSEND\\scontrino.txt"));
            using (StreamWriter outputFile = new StreamWriter(percorsowinecr + "\\TOSEND\\scontrino.txt"))
            {
                foreach (string line in commandi)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        private void InvioScontrinoRCH(List<ElementsScontrino> ScontrinoRow, double TotaleSconti, decimal PagamentoContante, decimal PagamentoCarta, decimal PagamentoNonRiscosso, List<FbScontrini> listasco, string infoscontrino)
        {
            List<string> commandi = new List<string>();
            commandi.Add("=K");
            commandi.Add("=C1");


            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                if (listasco.Count == 1)
                {
                    commandi.Add("=\"/?A/(Rif. Scontrino N. " + listasco[0].LOTTONUMERO + ")");
                }
                else
                {
                    commandi.Add("=\"/?A/(Rif. Scontrini)");
                    foreach (FbScontrini nscontrino in listasco)
                    {
                        commandi.Add("=\"/?A/(N. " + nscontrino.LOTTONUMERO + ")");
                    }
                }
            }

            decimal TotalePagamento = PagamentoContante + PagamentoCarta + PagamentoNonRiscosso;

            foreach (ElementsScontrino item in ScontrinoRow)
            {
                commandi.Add("=R" + item.rep + "/$" + (item.prezzo * item.qty).ToString("0.00").Replace(",", "") + "/(" + item.qty.ToString() + " " + item.desc.Replace("*", "").Replace("\'", "") + ")");
            }

            if (TotaleSconti > 0)
            {
                commandi.Add("=S");
                commandi.Add("=V+/$" + TotaleSconti.ToString("0.00").Replace(",", "") + "/(MAGG.)");
            }
            else if (TotaleSconti < 0)
            {
                commandi.Add("=S");
                commandi.Add("=V-/$" + TotaleSconti.ToString("0.00").Replace(",", "").Replace("-", "") + "/(SCONTO)");
            }

            if (infoscontrino != "" && scontrinoparlante == "true")
            {
                commandi.Add("=\"/?C/(" + infoscontrino + ")");
            }

            if (PagamentoContante > 0)
            {
                commandi.Add("=T1/$" + PagamentoContante.ToString("0.00").Replace(",", ""));
            }
            if (PagamentoCarta > 0)
            {
                commandi.Add("=T3/$" + PagamentoCarta.ToString("0.00").Replace(",", ""));
            }
            if (PagamentoNonRiscosso > 0)
            {
                commandi.Add("=T2/$" + PagamentoNonRiscosso.ToString("0.00").Replace(",", ""));
            }

            if (PagamentoContante == 0.00M && PagamentoCarta == 0.00M && PagamentoNonRiscosso == 0.00M)
            {
                commandi.Clear();
                MessageBox.Show("Errore stampa scontrino." +
                    "Errore calcolo pagamento", "PentaStart");
                return;
            }

            using (StreamWriter outputFile = new StreamWriter(percorsomultidriver + "\\TOSEND\\scontrino.txt"))
            {
                foreach (string line in commandi)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        private void InvioScontrinoEpson(List<ElementsScontrino> ScontrinoRow, double TotaleSconti, decimal PagamentoContante, decimal PagamentoCarta, decimal PagamentoNonRiscosso, List<FbScontrini> listasco, string infoscontrino)
        {
            List<string> commandi = new List<string>();
            commandi.Add("clear");
            commandi.Add("chiave reg");

            if (infoscontrino != "" && scontrinoparlante == "true")
            {
                commandi.Add("INP TERM=61");
                commandi.Add("INP ALFA=\'" + infoscontrino + "\',TERM=49");
            }

            foreach (ElementsScontrino item in ScontrinoRow)
            {
                commandi.Add("vend rep=" + item.rep + ", prezzo=" + (item.prezzo * item.qty).ToString("0.00").Replace(",", ".") + ", des=\'" + item.qty.ToString() + " " + item.desc.Replace("*", "").Replace("\'", "") + "\'");
            }

            if (TotaleSconti > 0)
            {
                commandi.Add("subt");
                commandi.Add("inp num = " + TotaleSconti.ToString("0.00").Replace(",", ".") + ", term = 34");
            }
            else if (TotaleSconti < 0)
            {
                commandi.Add("Sconto val=" + TotaleSconti.ToString("0.00").Replace(",", ".").Replace("-", "") + ",subtot");
            }


            if (listasco != null && listasco.Count > 0 && dettnsco == "true")
            {
                if (listasco.Count == 1)
                {
                    commandi.Add("PRMSG LINE=\'Rif. Scontrino N. " + listasco[0].LOTTONUMERO + "\'");
                }
                else
                {
                    commandi.Add("PRMSG LINE=\'Rif. Scontrini\'");
                    foreach (FbScontrini nscontrino in listasco)
                    {
                        commandi.Add("PRMSG LINE=\'N. " + nscontrino.LOTTONUMERO + "\'");
                    }
                }
            }


            if (PagamentoContante > 0)
            {
                commandi.Add("chius t=1, imp=" + PagamentoContante.ToString("0.00").Replace(",", "."));

            }
            if (PagamentoCarta > 0)
            {
                commandi.Add("chius t=5, imp=" + PagamentoCarta.ToString("0.00").Replace(",", "."));
            }
            if (PagamentoNonRiscosso > 0)
            {
                commandi.Add("chius t=2, imp=" + PagamentoNonRiscosso.ToString("0.00").Replace(",", "."));
            }

            if (PagamentoContante == 0.00M && PagamentoCarta == 0.00M && PagamentoNonRiscosso == 0.00M)
            {
                commandi.Clear();
                MessageBox.Show("Errore stampa scontrino." +
                    "Errore calcolo pagamento", "PentaStart");
                return;
            }
            //MessageBox.Show(Path.Combine(percorsowinecr, "\\TOSEND\\scontrino.txt"));
            using (StreamWriter outputFile = new StreamWriter(percorsofpmate + "\\TOSEND\\scontrino.txt"))
            {
                foreach (string line in commandi)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        private void OpenDatabase()
        {
            while (connopen == false)
            {
                try
                {
                    LoadDatabase();
                    connopen = true;
                    Log.WriteLog("Database aperto (" + percorsodb + ")");
                }
                catch (Exception ex)
                {
                    Log.WriteLog("Errore nella apertura del database: " + ex.ToString());
                    connopen = false;
                    MessageBox.Show("Errore: " + ex.ToString());
                }
            }
        }


        public static FbConnection LoadDatabase()
        {
            string ConnectionString = "User ID=" + userdb + ";Password=" + passdb + ";" + "Database=" + ipdb + ":" + percorsodb + "\\triLogis.fb20" + ";Charset=NONE;";
            Log.WriteLog("Connessione: " + ConnectionString);
            connection = new FbConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        static void LoadIni()
        {
            string iniexist = Application.StartupPath + "\\pentastart.ini";
            IniFile ini = new IniFile();
            if (System.IO.File.Exists(iniexist))
            {
                Log.WriteLog("Inizio caricamento PentaStart.ini (" + iniexist + ")");
                ini.Load(iniexist);
                userdb = ini.GetKeyValue("DB", "User");
                Log.WriteLog("UserDB: " + userdb);
                passdb = ini.GetKeyValue("DB", "Password");
                Log.WriteLog("PassDB: " + passdb);
                percorsomultidriver = ini.GetKeyValue("STAMPANTI", "PercorsoMultidriver");
                Log.WriteLog("Percorso Rch MultiDriver: " + percorsomultidriver);
                percorsowinecr = ini.GetKeyValue("STAMPANTI", "PercorsoWinEcr");
                Log.WriteLog("Percorso Ditron WinEcrCom: " + percorsowinecr);
                percorsofpmate = ini.GetKeyValue("STAMPANTI", "PercorsoFpMate");
                Log.WriteLog("Percorso Epson FpMate: " + percorsofpmate);
                percorsodb = ini.GetKeyValue("DB", "Percorso");
                Log.WriteLog("Percorso Database: " + percorsodb);
                ipdb = ini.GetKeyValue("DB", "IP");
                Log.WriteLog("Ip Database: " + ipdb);
                mct = ini.GetKeyValue("STAMPANTI", "MCT");
                Log.WriteLog("Stampante MCT: " + mct);
                ditron = ini.GetKeyValue("STAMPANTI", "Ditron");
                Log.WriteLog("Stampante Ditron: " + ditron);
                epson = ini.GetKeyValue("STAMPANTI", "Epson");
                Log.WriteLog("Stampante Epson: " + epson);
                postazione = ini.GetKeyValue("STAMPANTI", "Postazione");
                Log.WriteLog("Postazione PC: " + postazione);
                dettnsco = ini.GetKeyValue("STAMPANTI", "DettaglioNScontrino");
                Log.WriteLog(dettnsco == "true" ? "Stampa Numero Scontrino (Riferimento scontrino): SI" : "Stampa Numero Scontrino (Riferimento scontrino): NO");
                dettcapi = ini.GetKeyValue("STAMPANTI", "DettaglioCapi");
                Log.WriteLog(dettcapi == "true" ? "Stampa Dettaglio Capi (Acconto/Recupero): SI" : "Stampa Dettaglio Capi (Acconto/Recupero): NO");
                scontrinoparlante = ini.GetKeyValue("STAMPANTI", "ScontrinoParlante");
                Log.WriteLog(scontrinoparlante == "true" ? "Stampa C.F/P.IVA (Scontrino Parlante): SI" : "Stampa C.F/P.IVA (Scontrino Parlante): NO");
                Log.WriteLog("Fine caricamento PentaStart.ini");
            }
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show();
            }
            else
                return;
        }

        private void ResetPostazioneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModificaIni("STAMPANTI", "Postazione", "0");

            FbCommand deletescontrino = new FbCommand("DROP TRIGGER NEW_SCONTRINOIMMEDIATO", connection);
            deletescontrino.ExecuteNonQuery();
            Log.WriteLog("Trigger cancellato correttamente");
            Application.Restart();
        }

        private static void ModificaIni(string percorso, string key, string valore)
        {
            string iniexist = Application.StartupPath + "\\pentastart.ini";
            IniFile ini = new IniFile();
            ini.Load(iniexist);
            ini.SetKeyValue(percorso, key, valore);
            ini.Save(iniexist);
            Log.WriteLog("Modifica PentaStart.ini: Percorso: " + percorso + " Key:" + key + " Valore: " + valore);
        }

        private void ImpostaPagamentiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImpostaPagamenti();
        }

        private static void ImpostaPagamenti()
        {
            FbCommand pag1 = new FbCommand("UPDATE TIPIPAGAMENTI SET TIPOPAGAMENTODESCRIZIONE = 'Contanti',TIPOPAGAMENTOATTIVO = 'True',TIPOPAGAMENTOABILITATO = 'True',TIPOPAGAMENTOIMMEDIATO = 'True',TIPOPAGAMENTOCODICE=1 WHERE TIPOPAGAMENTOID = 1 ;", connection);
            pag1.ExecuteNonQuery();

            FbCommand pag2 = new FbCommand("UPDATE TIPIPAGAMENTI SET TIPOPAGAMENTODESCRIZIONE = 'Pag. Elettronico',TIPOPAGAMENTOATTIVO = 'True',TIPOPAGAMENTOABILITATO = 'True',TIPOPAGAMENTOIMMEDIATO = 'False',TIPOPAGAMENTOCODICE=2 WHERE TIPOPAGAMENTOID = 2 ;", connection);
            pag2.ExecuteNonQuery();

            FbCommand pag3 = new FbCommand("UPDATE TIPIPAGAMENTI SET TIPOPAGAMENTODESCRIZIONE = 'Bonifico',TIPOPAGAMENTOATTIVO = 'True',TIPOPAGAMENTOABILITATO = 'True',TIPOPAGAMENTOIMMEDIATO = 'False',TIPOPAGAMENTOCODICE=2 WHERE TIPOPAGAMENTOID = 3 ;", connection);
            pag3.ExecuteNonQuery();

            FbCommand pag4 = new FbCommand("UPDATE TIPIPAGAMENTI SET TIPOPAGAMENTODESCRIZIONE = 'Non Riscosso',TIPOPAGAMENTOATTIVO = 'True',TIPOPAGAMENTOABILITATO = 'True',TIPOPAGAMENTOIMMEDIATO = 'False',TIPOPAGAMENTOCODICE=3 WHERE TIPOPAGAMENTOID = 4 ;", connection);
            pag4.ExecuteNonQuery();

            FbCommand pag6 = new FbCommand("UPDATE TIPIPAGAMENTI SET TIPOPAGAMENTOABILITATO = 'False' WHERE TIPOPAGAMENTOID > 5 AND TIPOPAGAMENTOID != 899;", connection);
            pag6.ExecuteNonQuery();

            Log.WriteLog("Pagamenti modificati con successo:");
            Log.WriteLog("Codice Pagamento Contanti: 1");
            Log.WriteLog("Codice Pagamento Elettronico: 2");
            Log.WriteLog("Codice Pagamento Non Riscosso: 3");
        }

        private void RedimensionaForm(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }
    }
}
