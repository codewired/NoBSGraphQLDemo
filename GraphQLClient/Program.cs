﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GraphQL.Common.Request;
using GraphQLClient.Entities;
using Newtonsoft.Json;

namespace GraphQLClient
{
    internal class Program
    {
        private const string GraphQlServerUrl = "https://localhost:44307/GraphQL";

        private static async Task Main(string[] args)
        {
            //var customers = await SimpleWebClientDemo();
            // var customer = await SimpleGraphQLClientDemo();
            var customer = await SimpleMutationClientDemo();
        }

        private static async Task<Customer> SimpleMutationClientDemo()
        {
            var customerRequest = new GraphQLRequest
            {
                Query = @"mutation ($customer:CustomerInput!) 
                            {
                                    createCustomer(customer: $customer) 
                                    {
                                    id
                                    name
                                    city
                                }
                      }",
                Variables = new
                {
                    customer = new
                    {
                        name = "me", address="1234 anywhere", city="buffalo grove"
                    }
                }
            };

            var graphQLClient = new GraphQL.Client.GraphQLClient(GraphQlServerUrl);
            var graphQLResponse = await graphQLClient.PostAsync(customerRequest);
            var customer = graphQLResponse.GetDataFieldAs<Customer>("createCustomer");
            return customer;

        }

        private static async Task<Customer> SimpleGraphQLClientDemo()
        {
            var customerRequest = new GraphQLRequest
            {
                Query = @"query Sample($customerId :Int, $portfolioId: Int, $present: Boolean!)
                            {
                              customer:customer(id:$customerId)
                              {
                                id  @include(if: $present)
                                address
                                 ...custFragment
                              }  
                            }
                            fragment custFragment on CustomerType
                            {
                              name
                              portfolio(id:$portfolioId)
                              {
                                name
                                id
                              }
                            }",
                Variables = new {portfolioId = 1, customerId = 2, present = false}
            };

            var graphQLClient = new GraphQL.Client.GraphQLClient(GraphQlServerUrl);
            var graphQLResponse = await graphQLClient.PostAsync(customerRequest);
            var customer = graphQLResponse.GetDataFieldAs<Customer>("customer");
            return customer;
        }

        private static async Task<List<Customer>> SimpleWebClientDemo()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(GraphQlServerUrl)
            };

            var response =
                await client.GetAsync(@" ? query={ customers { id name portfolios { id stocks { id name } }  } }");
            var stringResult = await response.Content.ReadAsStringAsync();
            var root = JsonConvert.DeserializeObject<RootObject>(stringResult);
            return root.Data.Customers;
        }
    }
}