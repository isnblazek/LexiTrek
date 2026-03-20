using LexiTrek.Shared.DTOs;

namespace LexiTrek.Web.Services;

public class DictionaryStateService
{
    private readonly DictionaryApiService _api;

    public DictionaryStateService(DictionaryApiService api) => _api = api;

    public List<DictionaryListDto> Dictionaries { get; private set; } = [];
    public Guid? SelectedDictionaryId { get; private set; }
    public DictionaryListDto? SelectedDictionary =>
        Dictionaries.FirstOrDefault(d => d.Id == SelectedDictionaryId);

    public event Action? OnChange;

    public async Task LoadAsync()
    {
        Dictionaries = await _api.GetDictionariesAsync();
        if (SelectedDictionaryId == null || Dictionaries.All(d => d.Id != SelectedDictionaryId))
        {
            SelectedDictionaryId = Dictionaries.FirstOrDefault()?.Id;
        }
        OnChange?.Invoke();
    }

    public void SetSelectedDictionary(Guid? id)
    {
        SelectedDictionaryId = id;
        OnChange?.Invoke();
    }
}
