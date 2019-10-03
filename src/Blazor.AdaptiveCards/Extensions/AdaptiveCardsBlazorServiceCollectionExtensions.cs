using AdaptiveCards;
using AdaptiveCards.Rendering.Html;
using System;
using Blazor.AdaptiveCards;
using Blazor.AdaptiveCards.ActionHandlers;
using Blazor.AdaptiveCards.Templating;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public class BlazorAdaptiveCardsBuilder
    {
        public BlazorAdaptiveCardsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }

    public static class AdaptiveCardsBlazorModelTemplateExtensions
    {
        public static BlazorAdaptiveCardsBuilder AddFileTemplate<TModel>(this BlazorAdaptiveCardsBuilder builder, string filepath)
        {
            builder.Services.AddSingleton<IModelTemplateProvider<TModel>>(new FileModelTemplateProvider<TModel>(filepath));

            return builder;
        }
    }

    public static class AdaptiveCardsBlazorServiceCollectionExtensions
    {
        public static BlazorAdaptiveCardsBuilder AddBlazorAdaptiveCards(this IServiceCollection services, Action<BlazorAdaptiveCardsOptions> configure = null)
        {
            var builder = new BlazorAdaptiveCardsBuilder(services);

            var options = new BlazorAdaptiveCardsOptions();
            configure?.Invoke(options);

            if (options.AdaptiveOpenUrlActionProvider != null)
            {
                AdaptiveCardActionCreators.CreateAdaptiveOpenUrlAction = options.AdaptiveOpenUrlActionProvider;
            }

            if (options.AdaptiveShowCardActionProvider != null)
            {
                AdaptiveCardActionCreators.CreateAdaptiveShowCardAction = options.AdaptiveShowCardActionProvider;
            }

            if (options.AdaptiveSubmitActionProvider != null)
            {
                AdaptiveCardActionCreators.CreateAdaptiveSubmitAction = options.AdaptiveSubmitActionProvider;
            }

            if (options.AdaptiveCardTemplatingProvider == null)
            {
                services.AddSingleton<IAdaptiveCardTemplatingProvider, ScribanCardBinder>();
            }

            AdaptiveCardRenderer.ActionTransformers.Register<AdaptiveOpenUrlAction>((action, tag, context) =>
            {
                AdaptiveCardActionCreators.CreateAdaptiveOpenUrlAction(action, tag, context);
            });

            AdaptiveCardRenderer.ActionTransformers.Register<AdaptiveShowCardAction>((action, tag, context) =>
            {
                AdaptiveCardActionCreators.CreateAdaptiveShowCardAction(action, tag, context);
            });

            AdaptiveCardRenderer.ActionTransformers.Register<AdaptiveSubmitAction>((action, tag, context) =>
            {
                AdaptiveCardActionCreators.CreateAdaptiveSubmitAction(action, tag, context);
            });

            services.AddSingleton<AdaptiveCardRenderer>();
            services.AddSingleton<AdaptiveOpenUrlActionAdapter>();
            services.TryAddSingleton<ISubmitActionHandler, DefaultSubmitActionHandler>();
            services.TryAddSingleton(typeof(IModelTemplateProvider<>), typeof(EmptyModelTemplateProvider<>));

            services.AddSingleton(options);

            return builder;
        }
    }
}
