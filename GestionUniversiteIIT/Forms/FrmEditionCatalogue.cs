using System.Data;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public class FrmEditionCatalogue : Form
{
    private readonly CatalogueService _service;
    private readonly DefinitionCatalogue _definition;
    private readonly int? _id;
    private readonly Dictionary<string, Control> _controles = new(StringComparer.Ordinal);

    public FrmEditionCatalogue(CatalogueService service, DefinitionCatalogue definition, int? id = null)
    {
        _service = service;
        _definition = definition;
        _id = id;
        ConstruireInterface();
        ChargerValeurs();
    }

    private void ConstruireInterface()
    {
        Text = _id.HasValue ? $"Modifier — {_definition.Titre}" : $"Ajouter — {_definition.Titre}";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        MinimumSize = new Size(490, 260);
        ClientSize = new Size(540, Math.Min(760, 165 + _definition.Champs.Count * 58));

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            ColumnCount = 2,
            RowCount = _definition.Champs.Count + 2,
            AutoScroll = true,
            BackColor = Color.White
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        root.Controls.Add(UiStyle.Titre(_id.HasValue ? "Modifier l'enregistrement" : "Nouvel enregistrement", 16), 0, 0);
        root.SetColumnSpan(root.GetControlFromPosition(0, 0)!, 2);

        var ligne = 1;
        foreach (var champ in _definition.Champs)
        {
            var label = new Label
            {
                Text = champ.Libelle + (champ.Obligatoire ? " *" : ""),
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                ForeColor = UiStyle.Texte,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Margin = new Padding(0, 8, 8, 8)
            };
            var control = CreerControle(champ);
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(4, 6, 4, 6);
            root.Controls.Add(label, 0, ligne);
            root.Controls.Add(control, 1, ligne);
            _controles.Add(champ.Nom, control);
            ligne++;
        }

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Margin = new Padding(0, 14, 0, 0),
            BackColor = Color.White
        };
        var annuler = UiStyle.BoutonArdoise("Annuler", 124, 40);
        var enregistrer = UiStyle.Bouton("Enregistrer", true);
        enregistrer.Click += (_, _) => Enregistrer();
        annuler.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(annuler);
        actions.Controls.Add(enregistrer);
        root.Controls.Add(actions, 0, ligne);
        root.SetColumnSpan(actions, 2);
        Controls.Add(root);
    }

    private Control CreerControle(ChampCatalogue champ)
    {
        switch (champ.Type)
        {
            case TypeChamp.Nombre:
                return new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = 1_000_000,
                    ThousandsSeparator = true,
                    DecimalPlaces = 0
                };
            case TypeChamp.Date:
                return new DateTimePicker
                {
                    Format = DateTimePickerFormat.Short,
                    ShowCheckBox = !champ.Obligatoire,
                    Checked = champ.Obligatoire
                };
            case TypeChamp.Heure:
                return new DateTimePicker
                {
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "HH:mm",
                    ShowUpDown = true
                };
            case TypeChamp.OuiNon:
                return new CheckBox { Text = "Oui", Checked = true, AutoSize = true };
            case TypeChamp.Liste:
                var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                var choices = _service.ChargerListe(champ.RequeteListe!);
                if (!champ.Obligatoire)
                {
                    var empty = choices.NewRow();
                    empty["Id"] = DBNull.Value;
                    empty["Libelle"] = "— Aucun —";
                    choices.Rows.InsertAt(empty, 0);
                }

                combo.DataSource = choices;
                combo.DisplayMember = "Libelle";
                combo.ValueMember = "Id";
                return combo;
            default:
                return new TextBox { BorderStyle = BorderStyle.FixedSingle };
        }
    }

    private void ChargerValeurs()
    {
        if (!_id.HasValue)
        {
            return;
        }

        var row = _service.Lire(_definition, _id.Value);
        if (row is null)
        {
            return;
        }

        foreach (var champ in _definition.Champs)
        {
            if (!row.Table.Columns.Contains(champ.Nom))
            {
                continue;
            }

            var value = row[champ.Nom];
            if (value is DBNull or null)
            {
                continue;
            }

            var control = _controles[champ.Nom];
            switch (champ.Type)
            {
                case TypeChamp.Nombre:
                    ((NumericUpDown)control).Value = Math.Clamp(Convert.ToDecimal(value), 0, 1_000_000);
                    break;
                case TypeChamp.Date:
                    var date = (DateTimePicker)control;
                    date.Value = Convert.ToDateTime(value);
                    date.Checked = true;
                    break;
                case TypeChamp.Heure:
                    ((DateTimePicker)control).Value = DateTime.Today.Add((TimeSpan)value);
                    break;
                case TypeChamp.OuiNon:
                    ((CheckBox)control).Checked = Convert.ToBoolean(value);
                    break;
                case TypeChamp.Liste:
                    ((ComboBox)control).SelectedValue = value;
                    break;
                default:
                    ((TextBox)control).Text = Convert.ToString(value) ?? "";
                    break;
            }
        }
    }

    private void Enregistrer()
    {
        try
        {
            var values = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var champ in _definition.Champs)
            {
                var control = _controles[champ.Nom];
                object? value = champ.Type switch
                {
                    TypeChamp.Nombre => Convert.ToInt32(((NumericUpDown)control).Value),
                    TypeChamp.Date => ((DateTimePicker)control).ShowCheckBox && !((DateTimePicker)control).Checked
                        ? null
                        : ((DateTimePicker)control).Value.Date,
                    TypeChamp.Heure => ((DateTimePicker)control).Value.TimeOfDay,
                    TypeChamp.OuiNon => ((CheckBox)control).Checked,
                    TypeChamp.Liste => LireSelection((ComboBox)control),
                    _ => ((TextBox)control).Text.Trim()
                };

                if (champ.Obligatoire && (value is null || value is string text && string.IsNullOrWhiteSpace(text)))
                {
                    MessageBox.Show($"Le champ « {champ.Libelle} » est obligatoire.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    control.Focus();
                    return;
                }

                values[$"@{champ.Nom}"] = value is string empty && string.IsNullOrWhiteSpace(empty) ? null : value;
            }

            _service.Enregistrer(_definition, _id, values);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Impossible d'enregistrer : {exception.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static object? LireSelection(ComboBox combo)
    {
        if (combo.SelectedValue is null || combo.SelectedValue is DBNull)
        {
            return null;
        }

        return combo.SelectedValue;
    }
}
