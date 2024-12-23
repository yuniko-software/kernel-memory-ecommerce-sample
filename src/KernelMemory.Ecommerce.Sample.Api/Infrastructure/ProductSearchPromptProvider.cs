using System.Globalization;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Prompts;

namespace KernelMemory.Ecommerce.Sample.Api.Infrastructure;

public class ProductSearchPromptProvider : IPromptProvider
{
    private readonly string _productSearchPrompt = """
                                           Facts: 
                                           {{$facts}} 
                                           ======
                                           Based only on the facts above, return a list of the top {{$searchResultsLimit}} most relevant products based on the user's query below.
                                           Products may have the same name but different IDs, descriptions, or other attributes.
                                           Include all relevant details for each product and limit the results to a maximum of {{$searchResultsLimit}} items.
                                           Ensure the response strictly follows the JSON format specified below.

                                           Do not use Markdown formatting in the response, as it will be deserialized into JSON.

                                           Response format:
                                           [
                                               {
                                                   "Id": "first product guid",
                                                   "Name": "product name",
                                                   "Description": "product description",
                                                   "Price": price,
                                                   "PriceCurrency": "currency code",
                                                   "SupplyAbility": supply ability,
                                                   "MinimumOrder": minimum order quantity
                                               },
                                               {
                                                   "Id": "second product guid",
                                                   "Name": "product name",
                                                   "Description": "product description",
                                                   "Price": price,
                                                   "PriceCurrency": "currency code",
                                                   "SupplyAbility": supply ability,
                                                   "MinimumOrder": minimum order quantity
                                               }
                                               ...
                                           ]

                                           If no products are found or the user's query is invalid, return an empty JSON array.

                                           Reply with JSON only. No additional comments or explanations.
                                           User: {{$input}}
                                           Products:
                                           """;

#pragma warning disable KMEXP00 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly EmbeddedPromptProvider _fallbackProvider = new();
#pragma warning restore KMEXP00 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public ProductSearchPromptProvider(int searchResultsLimit)
    {
        _productSearchPrompt = _productSearchPrompt.Replace(
            "{{$searchResultsLimit}}",
            searchResultsLimit.ToString(CultureInfo.InvariantCulture));
    }

    public string ReadPrompt(string promptName)
    {
        return promptName switch
        {
            Constants.PromptNamesAnswerWithFacts => _productSearchPrompt,
            _ => _fallbackProvider.ReadPrompt(promptName) // Fall back to the default
        };
    }
}
