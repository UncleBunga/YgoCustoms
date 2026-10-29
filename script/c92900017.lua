-- Slasher's Schism Boxing
local s,id=GetID()
function s.initial_effect(c)
    -- Activate: Add 1 Ritual Spell, then optional add 1 Ritual Monster
    local e1=Effect.CreateEffect(c)
    e1:SetCategory(CATEGORY_TOHAND+CATEGORY_SEARCH)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetCode(EVENT_FREE_CHAIN)
    e1:SetTarget(s.target)
    e1:SetOperation(s.activate)
    c:RegisterEffect(e1)
end

function s.spfilter(c)
    return c:IsType(TYPE_RITUAL) and c:IsType(TYPE_SPELL) and c:IsAbleToHand()
end

function s.mofilter(c)
    return c:IsType(TYPE_RITUAL) and c:IsType(TYPE_MONSTER) and c:IsAbleToHand()
end

function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(s.spfilter,tp,LOCATION_DECK,0,1,nil) end
    Duel.SetOperationInfo(0,CATEGORY_TOHAND,nil,1,tp,LOCATION_DECK)
end

function s.activate(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_ATOHAND)
    local g=Duel.SelectMatchingCard(tp,s.spfilter,tp,LOCATION_DECK,0,1,1,nil)
    if #g>0 and Duel.SendtoHand(g,nil,REASON_EFFECT)>0 then
        Duel.ConfirmCards(1-tp,g)
        -- Optional: Remove 1 Kill Counter for Ritual Monster
        if Duel.IsCanRemoveCounter(tp,1,0,0x199,1,REASON_EFFECT)
            and Duel.IsExistingMatchingCard(s.mofilter,tp,LOCATION_DECK,0,1,nil)
            and Duel.SelectYesNo(tp,aux.Stringid(id,0)) then
            Duel.BreakEffect()
            Duel.RemoveCounter(tp,1,0,0x199,1,REASON_EFFECT)
            Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_ATOHAND)
            local g2=Duel.SelectMatchingCard(tp,s.mofilter,tp,LOCATION_DECK,0,1,1,nil)
            if #g2>0 then
                Duel.SendtoHand(g2,nil,REASON_EFFECT)
                Duel.ConfirmCards(1-tp,g2)
            end
        end
    end
end