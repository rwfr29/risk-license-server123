using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RISKHackTool.src.PlayerCardsForm
{
    public partial class PlayerCardsForm : Form
    {
        private String playerColor;
        private IntPtr playerCardListAddr;
        private List<IntPtr> playerCardAddrs = new List<IntPtr>();

        byte[] buffer = new byte[Constants.MAX_BUFFER_SIZE];
        IntPtr bytesRead = IntPtr.Zero;

        Process riskProcess;

        public PlayerCardsForm(String playerColor, IntPtr playerAddr, Process riskProcess)
{
    InitializeComponent();
    Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(playerColor) + " Player's Cards";
    this.riskProcess = riskProcess;
    this.playerColor = playerColor;

    try
    {
        // Read the card list pointer from Player (offset 0x110)
        HelperFunctions.ReadProcessMemory(riskProcess.Handle,
            IntPtr.Add(playerAddr, PlayerOffsets.CARD_LIST_OFFSET),
            buffer, MemoryConstants.POINTER_BYTES, out bytesRead);

        playerCardListAddr = new IntPtr(BitConverter.ToInt64(buffer, 0));

        if (playerCardListAddr == IntPtr.Zero)
        {
            MessageBox.Show($"Player {playerColor} has no cards yet!\n\nMake sure you are in a match and have drawn cards.", "No Cards");
            return;
        }

        GetPlayerCards();
        SetPlayerCardsInForm();
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}", "Error");
    }
}

       private void GetPlayerCards()
{
    buffer = new byte[Constants.MAX_BUFFER_SIZE];

    // Read number of cards
    HelperFunctions.ReadProcessMemory(riskProcess.Handle,
        IntPtr.Add(playerCardListAddr, CardListOffsets.NUMBER_OF_CARDS_OFFSET),
        buffer, MemoryConstants.INT_BYTES, out bytesRead);
    int numberOfCards = BitConverter.ToInt32(buffer);

    if (numberOfCards == 0)
    {
        MessageBox.Show("This player has no cards yet.", "Info");
        return;
    }

    HelperFunctions.ReadProcessMemory(riskProcess.Handle,
        IntPtr.Add(playerCardListAddr, CardListOffsets.PHYSICAL_CARD_LIST_OFFSET),
        buffer, MemoryConstants.POINTER_BYTES, out bytesRead);
    IntPtr addrOfCurrCard = new IntPtr(BitConverter.ToInt64(buffer, 0));

    for (int i = 0; i < numberOfCards; i++)
    {
        HelperFunctions.ReadProcessMemory(riskProcess.Handle,
            IntPtr.Add(addrOfCurrCard, PhysicalCardListOffsets.FIRST_CARD_OFFSET + (i * MemoryConstants.POINTER_BYTES)),
            buffer, MemoryConstants.POINTER_BYTES, out bytesRead);
        IntPtr cardAddr = new IntPtr(BitConverter.ToInt64(buffer, 0));
        playerCardAddrs.Add(cardAddr);
    }
}

        private void SetPlayerCardsInForm()
        {
            for (int i = 0; i < playerCardAddrs.Count; ++i)
            {
                Panel cardTroopPanel = new Panel();
                ComboBox cardComboBox = new ComboBox();
                int cardComboBoxIndex = GetCardTypeIndexForComboBox(i);

                cardTroopPanel.BackgroundImage = GetCardBackGroundImageForComboBox(cardComboBoxIndex);
                cardTroopPanel.BackgroundImageLayout = ImageLayout.Stretch;
                cardTroopPanel.BorderStyle = BorderStyle.FixedSingle;
                cardTroopPanel.Location = new Point(21 + i * 110, 35);
                cardTroopPanel.Size = new Size(94, 94);
                cardTroopPanel.Name = "card_troop_panel_" + i.ToString();

                cardComboBox.FormattingEnabled = true;
                cardComboBox.Items.AddRange(new object[] { CardType.Any, CardType.Infantry,
                    CardType.Calvary, CardType.Artillery });
                cardComboBox.Location = new Point(21 + i * 110, 147);
                cardComboBox.Name = "card_" + i.ToString();
                cardComboBox.Size = new Size(94, 94);
                cardComboBox.TabIndex = 1;
                cardComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                cardComboBox.SelectedIndex = cardComboBoxIndex;
                cardComboBox.SelectedIndexChanged += SetCardTypeComboBox_IndexChanged;

                Controls.Add(cardTroopPanel);
                Controls.Add(cardComboBox);
            }
        }

        private int GetCardTypeIndexForComboBox(int i)
        {
            buffer = new byte[Constants.MAX_BUFFER_SIZE];
            HelperFunctions.ReadProcessMemory(riskProcess.Handle, 
                IntPtr.Add(playerCardAddrs[i], CardOffsets.CARD_TYPE_OFFSET),
                buffer, MemoryConstants.INT_BYTES, out bytesRead);

            return BitConverter.ToInt32(buffer);
        }

        private Image GetCardBackGroundImageForComboBox(int comboBoxSelectedIndex)
        {
            switch (comboBoxSelectedIndex)
            {
                case (int)CardType.Any:
                    return Properties.Resources.AnyTroopImage;
                case (int)CardType.Infantry:
                    return Properties.Resources.InfantryImage;
                case (int)CardType.Calvary:
                    return Properties.Resources.CavalryImage;
                case (int)CardType.Artillery:
                    return Properties.Resources.ArtilleryImage;
                default:
                    throw new Exception("Read invalid card type");
            }
        }

        private void SetCardTypeComboBox_IndexChanged(object sender, EventArgs e)
        {
            ComboBox cardComboBox = (ComboBox)sender;
            int cardIndex = int.Parse(cardComboBox.Name.Split("_")[1]);

            buffer = new byte[Constants.MAX_BUFFER_SIZE];
            HelperFunctions.WriteProcessMemory(riskProcess.Handle, 
                IntPtr.Add(playerCardAddrs[cardIndex], CardOffsets.CARD_TYPE_OFFSET),
                BitConverter.GetBytes(cardComboBox.SelectedIndex), MemoryConstants.INT_BYTES, out bytesRead);

            Panel cardTroopPanel = (Panel)Controls.Find("card_troop_panel_" + cardIndex.ToString(), true)[0];
            cardTroopPanel.BackgroundImage = GetCardBackGroundImageForComboBox(cardComboBox.SelectedIndex);
            Update();
        }
    }
}